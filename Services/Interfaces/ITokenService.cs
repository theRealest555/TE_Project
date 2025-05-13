using TE_Project.Entities;

namespace TE_Project.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user, string? deviceInfo = null, string? ipAddress = null);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(string userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<IEnumerable<UserToken>> GetActiveUserTokensAsync(string userId);
    }
}