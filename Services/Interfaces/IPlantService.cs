using TE_Project.DTOs.Plant;
using TE_Project.Entities;

namespace TE_Project.Services.Interfaces
{
    public interface IPlantService
    {
        Task<IEnumerable<PlantDto>> GetAllPlantsAsync();
        Task<PlantDto?> GetPlantByIdAsync(int id);
    }
}