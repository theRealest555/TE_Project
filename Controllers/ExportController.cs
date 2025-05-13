using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TE_Project.DTOs.Export;
using TE_Project.Entities;
using TE_Project.Services.Interfaces;

namespace TE_Project.Controllers
{
    /// <summary>
    /// Controller for exporting data to Excel
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;
        private readonly IAuthService _authService;
        private readonly ILogger<ExportController> _logger;

        public ExportController(
            IExportService exportService,
            IAuthService authService,
            ILogger<ExportController> logger)
        {
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an Excel report based on the specified format and plant
        /// </summary>
        /// <param name="exportDto">Export configuration</param>
        /// <returns>Excel file</returns>
        /// <response code="200">Returns the Excel file</response>
        /// <response code="400">If the input is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to the requested data</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportDto exportDto)
        {
            try
            {
                // Determine user's plant and role
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID is missing or invalid." });
                }

                var userProfile = await _authService.GetUserProfileAsync(userId);
                bool isSuperAdmin = User.IsInRole(AdminRole.SuperAdmin);

                // Validate access based on plant
                if (!isSuperAdmin && exportDto.PlantId.HasValue && exportDto.PlantId != userProfile.PlantId)
                {
                    return Forbid();
                }

                // Generate the report
                var fileResult = await _exportService.GenerateExcelReportAsync(
                    exportDto,
                    userProfile.PlantId,
                    isSuperAdmin);

                return fileResult;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw; // Let middleware handle it
            }
        }
    }
}