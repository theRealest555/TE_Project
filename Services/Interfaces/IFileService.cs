using Microsoft.AspNetCore.Http;
using TE_Project.Enums;

namespace TE_Project.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, int plantId, FileType fileType, string fileName);
        string GetFileNameForSaving(string originalFileName, FileType fileType);
        string GetNewFolderPath(int plantId, FileType fileType);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]?> GetFileAsync(string filePath);
    }
}