using Microsoft.EntityFrameworkCore;
using TE_Project.Data;
using TE_Project.Entities;
using TE_Project.Repositories.Interfaces;

namespace TE_Project.Repositories
{
    public class PlantRepository : RepositoryBase<Plant>, IPlantRepository
    {
        public PlantRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Plant?> GetPlantWithAdminsAsync(int id)
        {
            return await _dbSet.AsNoTracking()
                .Include(p => p.Admins)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Plant?> GetPlantWithSubmissionsAsync(int id)
        {
            return await _dbSet.AsNoTracking()
                .Include(p => p.Submissions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Plant>> GetAllWithSubmissionCountAsync()
        {
            return await _dbSet.AsNoTracking()
                .Include(p => p.Submissions)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}