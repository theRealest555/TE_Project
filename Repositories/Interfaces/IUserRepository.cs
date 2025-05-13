using TE_Project.Entities;

namespace TE_Project.Repositories.Interfaces
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetByEmailAsync(string email, bool includeRoles = false);
        Task<User?> GetByIdWithPlantAsync(string id);
        Task<IEnumerable<User>> GetAdminsByPlantIdAsync(int plantId);
        Task<bool> IsInRoleAsync(User user, string role);
    }
}