using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TE_Project.Configurations;
using TE_Project.Data;
using TE_Project.Entities;
using TE_Project.Repositories.Interfaces;
using TE_Project.Services.Interfaces;

namespace TE_Project.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            AppDbContext context,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<TokenService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateTokenAsync(User user, string? deviceInfo = null, string? ipAddress = null)
        {
            var roles = await _userRepository.IsInRoleAsync(user, AdminRole.SuperAdmin) 
                ? new[] { AdminRole.SuperAdmin } 
                : new[] { AdminRole.RegularAdmin };

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName),
                new Claim("PlantId", user.PlantId.ToString()),
                new Claim("IsSuperAdmin", user.IsSuperAdmin.ToString()),
                new Claim("RequirePasswordChange", user.RequirePasswordChange.ToString())
            };
            
            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is not configured properly.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Trim device info to prevent database field overflow
            if (deviceInfo?.Length > 100)
            {
                deviceInfo = deviceInfo[0..100];
            }
            
            // Trim IP address to prevent database field overflow
            if (ipAddress?.Length > 45)
            {
                ipAddress = ipAddress[0..45];
            }

            // Save token to database
            var userToken = new UserToken
            {
                Token = tokenString,
                UserId = user.Id,
                ExpiresAt = expires,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated token for user {UserId} from IP {IpAddress}", user.Id, ipAddress);
            return tokenString;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(ut => ut.Token == token);

            if (userToken == null)
            {
                _logger.LogWarning("Attempted to revoke non-existent token");
                return false;
            }

            userToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked token for user {UserId}", userToken.UserId);
            return true;
        }

        public async Task<bool> RevokeAllUserTokensAsync(string userId)
        {
            var userTokens = await _context.UserTokens
                .Where(ut => ut.UserId == userId && !ut.IsRevoked)
                .ToListAsync();

            if (!userTokens.Any())
            {
                _logger.LogInformation("No active tokens to revoke for user {UserId}", userId);
                return true;
            }

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked {Count} tokens for user {UserId}", userTokens.Count, userId);
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(ut => ut.Token == token);

            if (userToken == null)
            {
                _logger.LogWarning("Token validation failed: Token not found in database");
                return false;
            }

            if (userToken.IsRevoked)
            {
                _logger.LogWarning("Token validation failed: Token has been revoked for user {UserId}", userToken.UserId);
                return false;
            }

            if (userToken.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Token validation failed: Token has expired for user {UserId}", userToken.UserId);
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<UserToken>> GetActiveUserTokensAsync(string userId)
        {
            return await _context.UserTokens
                .AsNoTracking()
                .Where(ut => ut.UserId == userId && 
                             !ut.IsRevoked && 
                             ut.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(ut => ut.CreatedAt)
                .ToListAsync();
        }
    }
}