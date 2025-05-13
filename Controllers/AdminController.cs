using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TE_Project.DTOs.Auth;
using TE_Project.Entities;
using TE_Project.Services.Interfaces;

namespace TE_Project.Controllers
{
    /// <summary>
    /// Controller for admin-specific operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AdminRole.SuperAdmin)]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAuthService authService,
            ILogger<AdminController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all admin users (Super Admin only)
        /// </summary>
        /// <returns>List of admin users</returns>
        /// <response code="200">Returns the list of admin users</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a Super Admin</response>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAdmins()
        {
            try
            {
                var adminUsers = await _authService.GetAllAdminsAsync();
                return Ok(adminUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin users");
                throw; // Let middleware handle this
            }
        }

        /// <summary>
        /// Get admin users by plant ID (Super Admin only)
        /// </summary>
        /// <param name="plantId">Plant ID</param>
        /// <returns>List of admin users for the plant</returns>
        /// <response code="200">Returns the list of admin users for the plant</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a Super Admin</response>
        [HttpGet("users/plant/{plantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAdminsByPlant(int plantId)
        {
            try
            {
                var adminUsers = await _authService.GetAdminsByPlantIdAsync(plantId);
                return Ok(adminUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin users for plant {PlantId}", plantId);
                throw; // Let middleware handle this
            }
        }

        /// <summary>
        /// Reset an admin's password (Super Admin only)
        /// </summary>
        /// <param name="userId">User ID to reset password for</param>
        /// <returns>Success message</returns>
        /// <response code="200">Returns success message with the new password</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a Super Admin</response>
        /// <response code="404">If the user is not found</response>
        [HttpPost("reset-password/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            try
            {
                var (success, message, newPassword) = await _authService.ResetPasswordAsync(userId);
                
                if (!success)
                {
                    if (message == "User not found")
                        return NotFound(new { message });
                        
                    return BadRequest(new { message });
                }
                
                return Ok(new { message, newPassword });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                throw; // Let middleware handle this
            }
        }

        /// <summary>
        /// Delete an admin user (Super Admin only)
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Returns success message</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a Super Admin</response>
        /// <response code="404">If the user is not found</response>
        [HttpDelete("users/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAdmin(string userId)
        {
            try
            {
                var (success, message) = await _authService.DeleteAdminAsync(userId);
                
                if (!success)
                {
                    if (message == "User not found")
                        return NotFound(new { message });
                        
                    return BadRequest(new { message });
                }
                
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin user {UserId}", userId);
                throw; // Let middleware handle this
            }
        }
    }
}