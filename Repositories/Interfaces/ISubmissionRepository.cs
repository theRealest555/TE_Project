using TE_Project.Entities;

namespace TE_Project.Repositories.Interfaces
{
    public interface ISubmissionRepository : IRepositoryBase<Submission>
    {
        Task<IEnumerable<Submission>> GetByPlantIdAsync(int plantId);
        Task<Submission?> GetWithFilesAsync(int id);
        Task<IEnumerable<Submission>> GetWithFilesByPlantIdAsync(int plantId);
        Task<IEnumerable<Submission>> GetSubmissionsWithGreyCardAsync();
    }
}