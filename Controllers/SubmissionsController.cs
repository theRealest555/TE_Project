using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TE_Project.DTOs.Submission;
using TE_Project.Entities;
using TE_Project.Services.Interfaces;

namespace TE_Project.Controllers
{
    /// <summary>
    /// Controller for managing submissions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // Removed [Authorize] from here
    [Produces("application/json")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly IAuthService _authService;
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(
            ISubmissionService submissionService,
            IAuthService authService,
            ILogger<SubmissionsController> logger)
        {
            _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new submission with uploaded files
        /// </summary>
        /// <param name="model">Submission data and files</param>
        /// <returns>Created submission ID</returns>
        /// <response code="201">Returns the newly created submission ID</response>
        /// <response code="400">If the submission data is invalid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // Removed [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // Removed [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromForm] SubmissionDto model)
        {
            // Removed user validation logic since submissions are now public
            try
            {
                var submission = await _submissionService.CreateSubmissionAsync(model);
                
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = submission.Id }, 
                    new { message = "Submission created successfully", id = submission.Id }
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating submission for plant {PlantId}", model.PlantId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred while creating submission for plant {model.PlantId}." });
            }
        }

        /// <summary>
        /// Gets a submission by ID
        /// </summary>
        /// <param name="id">Submission ID</param>
        /// <returns>Submission details</returns>
        /// <response code="200">Returns the submission</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to the submission's plant</response>
        /// <response code="404">If the submission is not found</response>
        [HttpGet("{id}")]
        [Authorize] // Added authorization here
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionResponseDto>> GetById(int id)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id);
            
            if (submission == null)
                return NotFound(new { message = "Submission not found" });

            // Verify the user has access to this submission's plant
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User identity not found");
                return Unauthorized(new { message = "User identity not found" });
            }

            var hasAccess = await _authService.ValidateAdminAccessToPlantAsync(userId!, submission.PlantId);
            if (!hasAccess)
                return Forbid();

            return Ok(submission);
        }

        [HttpGet("plant/{plantId}")]
        [Authorize] // Added authorization here
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetByPlantId(int plantId)
        {
            // Verify the user has access to this plant
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User identity not found");
                return Unauthorized(new { message = "User identity not found" });
            }

            var hasAccess = await _authService.ValidateAdminAccessToPlantAsync(userId, plantId);
            if (!hasAccess)
                return Forbid();

            var submissions = await _submissionService.GetSubmissionsByPlantIdAsync(plantId);
            return Ok(submissions);
        }

        /// <summary>
        /// Gets all submissions (Super Admin only)
        /// </summary>
        /// <returns>List of all submissions</returns>
        /// <response code="200">Returns all submissions</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a Super Admin</response>
        [HttpGet]
        [Authorize(Roles = AdminRole.SuperAdmin)]  // This already had role-specific authorization
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetAll()
        {
            var submissions = await _submissionService.GetAllSubmissionsAsync();
            return Ok(submissions);
        }
    }
}