using Microsoft.AspNetCore.Identity;
using TE_Project.DTOs.Auth;
using TE_Project.Entities;

namespace TE_Project.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(IdentityResult result, string userId)> RegisterAdminAsync(RegisterAdminDto model);
        Task<(bool success, string token, string fullName, bool requirePasswordChange)> LoginAsync(LoginDto model);
        Task<(bool success, string message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<UserDto> GetUserProfileAsync(string userId);
        Task<bool> ValidateAdminAccessToPlantAsync(string userId, int plantId);
        Task<(bool success, string message, string newPassword)> ResetPasswordAsync(string userId);
        Task<(bool success, string message)> DeleteAdminAsync(string userId);
        Task<(bool success, string message)> UpdateAdminAsync(string userId, UpdateAdminDto model);
        Task<IEnumerable<UserDto>> GetAllAdminsAsync(
            string? email = null, 
            string? fullName = null, 
            int? plantId = null,
            string? plantName = null,
            bool? isSuperAdmin = null,
            bool? requirePasswordChange = null);
        Task<IEnumerable<UserDto>> GetAdminsByPlantIdAsync(int plantId);
    }
}