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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    }
}