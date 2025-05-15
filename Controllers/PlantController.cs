using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TE_Project.DTOs.Plant;
using TE_Project.Entities;
using TE_Project.Services.Interfaces;

namespace TE_Project.Controllers
{
    /// <summary>
    /// Controller for managing plants
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PlantsController : ControllerBase
    {
        private readonly IPlantService _plantService;
        private readonly ILogger<PlantsController> _logger;

        public PlantsController(
            IPlantService plantService,
            ILogger<PlantsController> logger)
        {
            _plantService = plantService ?? throw new ArgumentNullException(nameof(plantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all plants
        /// </summary>
        /// <returns>List of plants</returns>
        /// <response code="200">Returns all plants</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PlantDto>>> GetAll()
        {
            var plants = await _plantService.GetAllPlantsAsync();
            return Ok(plants);
        }

        /// <summary>
        /// Gets a plant by ID
        /// </summary>
        /// <param name="id">Plant ID</param>
        /// <returns>Plant details</returns>
        /// <response code="200">Returns the plant</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the plant is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlantDto>> GetById(int id)
        {
            var plant = await _plantService.GetPlantByIdAsync(id);
            
            if (plant == null)
                return NotFound(new { message = "Plant not found" });
                
            return Ok(plant);
        }

        /// <summary>
        /// Creates a new plant (Super Admin only)
        /// </summary>
        /// <param name="model">Plant details</param>
        /// <returns>Newly created plant</returns>
        [HttpPost]
        [Authorize(Roles = AdminRole.SuperAdmin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PlantDto>> Create([FromBody] CreatePlantDto model)
        {
            try
            {
                var plant = await _plantService.CreatePlantAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = plant.Id }, plant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plant");
                throw; // Let middleware handle this
            }
        }

        /// <summary>
        /// Updates a plant (Super Admin only)
        /// </summary>
        /// <param name="id">Plant ID</param>
        /// <param name="model">Updated plant details</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = AdminRole.SuperAdmin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePlantDto model)
        {
            try
            {
                var (success, message) = await _plantService.UpdatePlantAsync(id, model);
                
                if (!success)
                {
                    if (message == "Plant not found")
                        return NotFound(new { message });
                        
                    return BadRequest(new { message });
                }
                
                return Ok(new { message = "Plant updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plant {PlantId}", id);
                throw; // Let middleware handle this
            }
        }

        /// <summary>
        /// Deletes a plant (Super Admin only)
        /// </summary>
        /// <param name="id">Plant ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = AdminRole.SuperAdmin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, message) = await _plantService.DeletePlantAsync(id);
                
                if (!success)
                {
                    if (message == "Plant not found")
                        return NotFound(new { message });
                    
                    if (message == "Cannot delete plant with associated users or submissions")
                        return BadRequest(new { message });
                        
                    return BadRequest(new { message });
                }
                
                return Ok(new { message = "Plant deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plant {PlantId}", id);
                throw; // Let middleware handle this
            }
        }
    }
}