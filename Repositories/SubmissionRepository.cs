using Microsoft.EntityFrameworkCore;
using TE_Project.Data;
using TE_Project.Entities;
using TE_Project.Repositories.Interfaces;

namespace TE_Project.Repositories
{
    public class SubmissionRepository : RepositoryBase<Submission>, ISubmissionRepository
    {
        public SubmissionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Submission>> GetByPlantIdAsync(int plantId)
        {
            return await _dbSet.AsNoTracking()
                .Where(s => s.PlantId == plantId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Submission?> GetWithFilesAsync(int id)
        {
            return await _dbSet.AsNoTracking()
                .Include(s => s.Files)
                .Include(s => s.Plant)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Submission>> GetWithFilesByPlantIdAsync(int plantId)
        {
            return await _dbSet.AsNoTracking()
                .Include(s => s.Files)
                .Include(s => s.Plant)
                .Where(s => s.PlantId == plantId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsWithGreyCardAsync()
        {
            return await _dbSet.AsNoTracking()
                .Include(s => s.Plant)
                .Where(s => !string.IsNullOrEmpty(s.GreyCard))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}