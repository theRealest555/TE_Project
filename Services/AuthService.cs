using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TE_Project.DTOs.Auth;
using TE_Project.Entities;
using TE_Project.Repositories.Interfaces;
using TE_Project.Services.Interfaces;
using System.Text;


namespace TE_Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<User> userManager,
            IUserRepository userRepository,
            IPlantRepository plantRepository,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<(IdentityResult result, string userId)> RegisterAdminAsync(RegisterAdminDto model)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(model.Email))
                return (IdentityResult.Failed(new IdentityError { Description = "Email is required" }), string.Empty);

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(model.Email);
            if (existingUser != null)
                return (IdentityResult.Failed(new IdentityError { Description = "Email is already registered" }), string.Empty);

            // Determine and validate password
            string password = DeterminePassword(model);
            var passwordErrors = ValidatePassword(password);
            if (passwordErrors.Any())
                return (IdentityResult.Failed(passwordErrors.Select(e => new IdentityError { Description = e }).ToArray()), string.Empty);

            // Create user entity
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PlantId = model.PlantId,
                IsSuperAdmin = model.IsSuperAdmin,
                EmailConfirmed = true,
                TEID = model.TEID,
                RequirePasswordChange = true
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, password);

            // Add role if creation is successful
            if (result.Succeeded)
            {
                var roleName = model.IsSuperAdmin ? AdminRole.SuperAdmin : AdminRole.RegularAdmin;
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to add user {UserId} to role {Role}: {Errors}", 
                        user.Id, roleName, string.Join(", ", roleResult.Errors));
                        
                    // Cleanup the user if role assignment fails
                    await _userManager.DeleteAsync(user);
                    return (roleResult, string.Empty);
                }
                
                _logger.LogInformation("Created new admin user {Email} with ID {UserId}", model.Email, user.Id);
            }
            else
            {
                _logger.LogWarning("Failed to create admin user {Email}: {Errors}", 
                    model.Email, string.Join(", ", result.Errors));
            }

            return (result, user.Id);
        }

        public async Task<(bool success, string token, string fullName, bool requirePasswordChange)> LoginAsync(LoginDto model)
        {
            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Email} not found", model.Email);
                return (false, string.Empty, string.Empty, false);
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}", model.Email);
                return (false, string.Empty, string.Empty, false);
            }

            // Get device and IP info
            var deviceInfo = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var token = await _tokenService.GenerateTokenAsync(user, deviceInfo, ipAddress);
            
            _logger.LogInformation("User {Email} logged in successfully", model.Email);
            return (true, token, user.FullName, user.RequirePasswordChange);
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Change password failed: User ID {UserId} not found", userId);
                return (false, "User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Change password failed for user {Email}: {Errors}", 
                    user.Email, string.Join(", ", result.Errors));
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (user.RequirePasswordChange)
            {
                user.RequirePasswordChange = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            _logger.LogInformation("Password changed successfully for user {Email}", user.Email);
            return (true, "Password changed successfully");
        }

        public async Task<UserDto> GetUserProfileAsync(string userId)
        {
            var user = await _userRepository.GetByIdWithPlantAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PlantId = user.PlantId,
                PlantName = user.Plant?.Name,
                IsSuperAdmin = user.IsSuperAdmin,
                RequirePasswordChange = user.RequirePasswordChange,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> ValidateAdminAccessToPlantAsync(string userId, int plantId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Super admins can access any plant
            if (user.IsSuperAdmin)
                return true;

            // Regular admins can only access their assigned plant
            return user.PlantId == plantId;
        }

        public async Task<(bool success, string message, string newPassword)> ResetPasswordAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Reset password failed: User ID {UserId} not found", userId);
                return (false, "User not found", string.Empty);
            }

            // Generate a new random password
            var newPassword = GenerateRandomPassword();
            
            // Create reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to reset password for user {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors));
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), string.Empty);
            }
            
            // Set flag to require password change on next login
            user.RequirePasswordChange = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            
            // Revoke all active tokens for the user
            await _tokenService.RevokeAllUserTokensAsync(userId);
            
            _logger.LogInformation("Password reset successfully for user {UserId}", userId);
            return (true, "Password reset successfully", newPassword);
        }

        public async Task<(bool success, string message)> DeleteAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Delete admin failed: User ID {UserId} not found", userId);
                return (false, "User not found");
            }
            
            // Revoke all tokens
            await _tokenService.RevokeAllUserTokensAsync(userId);
            
            // Delete user
            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete admin user {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors));
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            
            _logger.LogInformation("Admin user {UserId} deleted successfully", userId);
            return (true, "Admin deleted successfully");
        }

        public async Task<IEnumerable<UserDto>> GetAllAdminsAsync()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Plant)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            
            var result = new List<UserDto>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    PlantId = user.PlantId,
                    PlantName = user.Plant?.Name,
                    IsSuperAdmin = user.IsSuperAdmin,
                    RequirePasswordChange = user.RequirePasswordChange,
                    Roles = roles.ToList()
                });
            }
            
            return result;
        }

        public async Task<IEnumerable<UserDto>> GetAdminsByPlantIdAsync(int plantId)
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Plant)
                .Where(u => u.PlantId == plantId)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            
            var result = new List<UserDto>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    PlantId = user.PlantId,
                    PlantName = user.Plant?.Name,
                    IsSuperAdmin = user.IsSuperAdmin,
                    RequirePasswordChange = user.RequirePasswordChange,
                    Roles = roles.ToList()
                });
            }
            
            return result;
        }

        private string DeterminePassword(RegisterAdminDto model)
        {
            // Use provided password, or fallback to generated password
            return !string.IsNullOrEmpty(model.Password)
                ? model.Password
                : GenerateStrongPassword(model.TEID);
        }

        private string GenerateStrongPassword(string baseValue)
        {
            // Generate a strong password based on TEID with additional random elements
            return $"{baseValue}A1!{Guid.NewGuid().ToString("N")[..8]}";
        }

        private string GenerateRandomPassword()
        {
            // Generate a random strong password with at least 12 characters
            var random = new Random();
            const string upperChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijkmnopqrstuvwxyz";
            const string digitChars = "23456789";
            const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            
            var password = new StringBuilder();
            
            // Add at least one of each required character type
            password.Append(upperChars[random.Next(upperChars.Length)]);
            password.Append(lowerChars[random.Next(lowerChars.Length)]);
            password.Append(digitChars[random.Next(digitChars.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);
            
            // Add 8 more random characters
            const string allChars = upperChars + lowerChars + digitChars + specialChars;
            for (int i = 0; i < 8; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }
            
            // Shuffle the password
            return new string(password.ToString().OrderBy(x => Guid.NewGuid()).ToArray());
        }

        private List<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("Password must be at least 8 characters long");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Password must contain at least one uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Password must contain at least one lowercase letter");

            if (!Regex.IsMatch(password, @"[0-9]"))
                errors.Add("Password must contain at least one number");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                errors.Add("Password must contain at least one special character");

            return errors;
        }
    }
}