using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TE_Project.Enums;

namespace TE_Project.Helpers
{
    public static class FileValidationHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const int MaxFileSizeInMb = 1;
        private const int MaxFileSizeInBytes = MaxFileSizeInMb * 1024 * 1024;

        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public static bool IsValidFileName(string fileName, FileType fileType)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            return fileType switch
            {
                FileType.Cin => Regex.IsMatch(nameWithoutExtension, RegexPatterns.CinPattern),
                FileType.PIC => Regex.IsMatch(nameWithoutExtension, RegexPatterns.PicPattern),
                FileType.CG => Regex.IsMatch(nameWithoutExtension, RegexPatterns.CGPattern),
                _ => false
            };
        }

        public static string GetUniqueFileName(string baseFileName)
        {
            return $"{Path.GetFileNameWithoutExtension(baseFileName)}_{Guid.NewGuid():N}{Path.GetExtension(baseFileName)}";
        }
    }
}