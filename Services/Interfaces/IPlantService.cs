using TE_Project.DTOs.Plant;

namespace TE_Project.Services.Interfaces
{
    public interface IPlantService
    {
        Task<IEnumerable<PlantDto>> GetAllPlantsAsync();
        Task<PlantDto?> GetPlantByIdAsync(int id);
        Task<PlantDto> CreatePlantAsync(CreatePlantDto model);
        Task<(bool success, string message)> UpdatePlantAsync(int id, UpdatePlantDto model);
        Task<(bool success, string message)> DeletePlantAsync(int id);
    }
}