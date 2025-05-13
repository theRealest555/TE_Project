using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TE_Project.Data;
using TE_Project.Entities;
using TE_Project.Repositories.Interfaces;

namespace TE_Project.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(AppDbContext context, UserManager<User> userManager) 
            : base(context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<User?> GetByEmailAsync(string email, bool includeRoles = false)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
                return null;
                
            if (includeRoles)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }
            
            return user;
        }

        public async Task<User?> GetByIdWithPlantAsync(string id)
        {
            return await _dbSet.AsNoTracking()
                .Include(u => u.Plant)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetAdminsByPlantIdAsync(int plantId)
        {
            return await _dbSet.AsNoTracking()
                .Where(u => u.PlantId == plantId)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<bool> IsInRoleAsync(User user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }
    }
}