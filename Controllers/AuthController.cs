using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TE_Project.DTOs.Auth;
using TE_Project.Entities;
using TE_Project.Services.Interfaces;

namespace TE_Project.Controllers
{
    /// <summary>
    /// Controller for authentication and user management operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        /// <response code="200">Returns the JWT token and user information</response>
        /// <response code="401">If the credentials are invalid</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var (success, token, fullName, requirePasswordChange) = await _authService.LoginAsync(model);

            if (!success)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new
            {
                token,
                user = fullName,
                requirePasswordChange
            });
        }

        /// <summary>
        /// Logs out the current user by revoking their JWT token
        /// </summary>
        /// <returns>Success message</returns>
        /// <response code="200">If the logout was successful</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            // Get current token from request
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await _tokenService.RevokeTokenAsync(token);
            }

            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Logs out the user from all devices by revoking all their active tokens
        /// </summary>
        /// <returns>Success message</returns>
        /// <response code="200">If the logout from all devices was successful</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAllDevices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            await _tokenService.RevokeAllUserTokensAsync(userId);

            return Ok(new { message = "Logged out from all devices" });
        }

        /// <summary>
        /// Registers a new admin user
        /// </summary>
        /// <param name="model">Admin registration details</param>
        /// <returns>Success message and user ID</returns>
        /// <response code="200">Returns success message and created user ID</response>
        /// <response code="400">If the input is invalid</response>
        /// <response code="403">If the user is not authorized to register an admin</response>
        [HttpPost("register")]
        [Authorize(Roles = AdminRole.SuperAdmin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto model)
        {
            try
            {
                var (result, userId) = await _authService.RegisterAdminAsync(model);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new
                    {
                        message = "Registration failed",
                        errors
                    });
                }

                return Ok(new
                {
                    message = "Admin registered successfully",
                    userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin registration");
                throw; // Let the middleware handle this
            }
        }

        /// <summary>
        /// Changes the password of the current user
        /// </summary>
        /// <param name="model">Change password details</param>
        /// <returns>Success message</returns>
        /// <response code="200">Returns success message if password was changed</response>
        /// <response code="400">If the input is invalid or current password is incorrect</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity not found" });

            var (success, message) = await _authService.ChangePasswordAsync(
                userId,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        /// <summary>
        /// Gets the profile information of the current user
        /// </summary>
        /// <returns>User profile information</returns>
        /// <response code="200">Returns the user profile</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity not found" });

            try
            {
                var userProfile = await _authService.GetUserProfileAsync(userId);
                return Ok(userProfile);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
        }

        /// <summary>
        /// Gets all active sessions (tokens) for the current user
        /// </summary>
        /// <returns>List of active sessions</returns>
        /// <response code="200">Returns list of active sessions</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("sessions")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetActiveSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity not found" });

            var activeSessions = await _tokenService.GetActiveUserTokensAsync(userId);
            
            return Ok(activeSessions.Select(s => new
            {
                s.Id,
                s.DeviceInfo,
                s.IpAddress,
                s.CreatedAt,
                s.ExpiresAt
            }));
        }
    }
}