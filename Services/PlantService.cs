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

        public async Task<PlantDto> CreatePlantAsync(CreatePlantDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException("Plant name is required");
            }
            
            var existingPlant = await _plantRepository.GetFirstOrDefaultAsync(p => p.Name == model.Name);
            if (existingPlant != null)
            {
                throw new ArgumentException($"A plant with the name '{model.Name}' already exists");
            }
            
            var plant = new Plant
            {
                Name = model.Name,
                Description = model.Description
            };
            
            await _plantRepository.AddAsync(plant);
            await _plantRepository.SaveChangesAsync();
            
            _logger.LogInformation("Created new plant {PlantId} with name {PlantName}", plant.Id, plant.Name);
            
            return MapToPlantDto(plant);
        }

        public async Task<(bool success, string message)> UpdatePlantAsync(int id, UpdatePlantDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return (false, "Plant name is required");
            }
            
            var plant = await _plantRepository.GetByIdAsync(id, trackChanges: true);
            if (plant == null)
            {
                _logger.LogWarning("Update plant failed: Plant ID {PlantId} not found", id);
                return (false, "Plant not found");
            }
            
            if (plant.Name != model.Name)
            {
                var existingPlant = await _plantRepository.GetFirstOrDefaultAsync(p => p.Name == model.Name);
                if (existingPlant != null && existingPlant.Id != id)
                {
                    return (false, $"A plant with the name '{model.Name}' already exists");
                }
            }
            
            plant.Name = model.Name;
            plant.Description = model.Description;
            
            await _plantRepository.SaveChangesAsync();
            
            _logger.LogInformation("Updated plant {PlantId} with name {PlantName}", plant.Id, plant.Name);
            
            return (true, "Plant updated successfully");
        }

        public async Task<(bool success, string message)> DeletePlantAsync(int id)
        {
            var plant = await _plantRepository.GetByIdAsync(id, includeProperties: "Admins,Submissions", trackChanges: true);
            if (plant == null)
            {
                _logger.LogWarning("Delete plant failed: Plant ID {PlantId} not found", id);
                return (false, "Plant not found");
            }
            
            if ((plant.Admins != null && plant.Admins.Any()) || 
                (plant.Submissions != null && plant.Submissions.Any()))
            {
                _logger.LogWarning("Cannot delete plant {PlantId} because it has associated users or submissions", id);
                return (false, "Cannot delete plant with associated users or submissions");
            }
            
            _plantRepository.Remove(plant);
            await _plantRepository.SaveChangesAsync();
            
            _logger.LogInformation("Deleted plant {PlantId}", id);
            
            return (true, "Plant deleted successfully");
        }

        private PlantDto MapToPlantDto(Plant plant)
        {
            return new PlantDto
            {
                Id = plant.Id,
                Name = plant.Name,
                Description = plant.Description
            };
        }
    }
}