using TE_Project.Entities;

namespace TE_Project.Repositories.Interfaces
{
    public interface IPlantRepository : IRepositoryBase<Plant>
    {
        Task<Plant?> GetPlantWithAdminsAsync(int id);
        Task<Plant?> GetPlantWithSubmissionsAsync(int id);
        Task<IEnumerable<Plant>> GetAllWithSubmissionCountAsync();
    }
}