using TE_Project.DTOs.Submission;
using TE_Project.Entities;

namespace TE_Project.Services.Interfaces
{
    public interface ISubmissionService
    {
        Task<Submission> CreateSubmissionAsync(SubmissionDto model);
        Task<SubmissionResponseDto?> GetSubmissionByIdAsync(int id);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByPlantIdAsync(int plantId);
        Task<IEnumerable<SubmissionResponseDto>> GetAllSubmissionsAsync();
    }
}