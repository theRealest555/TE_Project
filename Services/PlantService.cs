using Microsoft.Extensions.Logging;
using TE_Project.DTOs.Plant;
using TE_Project.Entities;
using TE_Project.Repositories.Interfaces;
using TE_Project.Services.Interfaces;

namespace TE_Project.Services
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepository;
        private readonly ILogger<PlantService> _logger;

        public PlantService(
            IPlantRepository plantRepository,
            ILogger<PlantService> logger)
        {
            _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PlantDto>> GetAllPlantsAsync()
        {
            var plants = await _plantRepository.GetAllAsync(
                orderBy: p => p.OrderBy(x => x.Name)
            );

            return plants.Select(MapToPlantDto);
        }

        public async Task<PlantDto?> GetPlantByIdAsync(int id)
        {
            var plant = await _plantRepository.GetByIdAsync(id);
            if (plant == null)
                return null;

            return MapToPlantDto(plant);
        }

        private PlantDto MapToPlantDto(Plant plant)
        {
            return new PlantDto
            {
                Id = plant.Id,
                Name = plant.Name
            };
        }
    }
}