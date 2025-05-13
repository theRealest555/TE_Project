using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TE_Project.Enums;
using TE_Project.Helpers;
using TE_Project.Services.Interfaces;

namespace TE_Project.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private const int MaxFilesPerFolder = 100;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> SaveFileAsync(IFormFile file, int plantId, FileType fileType, string fileName)
        {
            try
            {
                if (file == null)
                    throw new ArgumentNullException(nameof(file), "File cannot be null");

                if (!FileValidationHelper.IsValidImage(file))
                {
                    throw new ArgumentException("Invalid file. File must be an image (jpg, jpeg, png) and less than 5MB.");
                }

                // Double-check file extension security
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    throw new ArgumentException($"File extension {extension} is not allowed.");
                }

                var folderPath = GetNewFolderPath(plantId, fileType);
                
                // Create folder if it doesn't exist
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                
                // Extra security: Ensure we don't overwrite existing files
                if (File.Exists(filePath))
                {
                    // Add a unique suffix to avoid conflicts
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    fileName = $"{fileNameWithoutExt}_{Guid.NewGuid():N}{extension}";
                    filePath = Path.Combine(folderPath, fileName);
                }

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File {fileName} saved successfully to {folderPath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file {fileName} for plant {plantId}, file type {fileType}");
                throw new ApplicationException($"Error occurred while saving the file: {ex.Message}", ex);
            }
        }

        public string GetFileNameForSaving(string originalFileName, FileType fileType)
        {
            try
            {
                if (string.IsNullOrEmpty(originalFileName))
                    throw new ArgumentException("File name cannot be empty");

                var extension = Path.GetExtension(originalFileName);
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

                // Validate the file name according to type
                if (!FileValidationHelper.IsValidFileName(originalFileName, fileType))
                {
                    throw new ArgumentException($"File name does not match the pattern for {fileType}");
                }

                // Sanitize the filename to prevent path traversal
                var sanitizedName = SanitizeFileName(nameWithoutExtension);
                
                return $"{sanitizedName}{extension}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file name {originalFileName} for file type {fileType}");
                throw;
            }
        }

        public string GetNewFolderPath(int plantId, FileType fileType)
        {
            if (plantId <= 0)
            {
                throw new ArgumentException("Invalid plant ID");
            }
            
            var baseUploadFolder = Path.Combine(_environment.WebRootPath, "uploads");

            // Create plant folder if it doesn't exist
            var plantFolderPath = Path.Combine(baseUploadFolder, plantId.ToString());
            if (!Directory.Exists(plantFolderPath))
            {
                Directory.CreateDirectory(plantFolderPath);
            }

            // Create file type folder if it doesn't exist
            var fileTypeFolderName = fileType switch
            {
                FileType.Cin => "cin",
                FileType.PIC => "pic",
                FileType.CG => "grey_card",
                _ => throw new ArgumentException("Invalid file type")
            };

            var fileTypeFolderPath = Path.Combine(plantFolderPath, fileTypeFolderName);
            if (!Directory.Exists(fileTypeFolderPath))
            {
                Directory.CreateDirectory(fileTypeFolderPath);
            }

            // Find the last numbered folder or create folder 1
            var folders = Directory.GetDirectories(fileTypeFolderPath)
                .Select(d => new DirectoryInfo(d).Name)
                .Where(name => int.TryParse(name, out _))
                .Select(int.Parse)
                .OrderBy(num => num)
                .ToList();

            int folderNumber = 1;
            if (folders.Any())
            {
                folderNumber = folders.Last();
                var currentFolderPath = Path.Combine(fileTypeFolderPath, folderNumber.ToString());

                // Check if the current folder has reached the maximum file limit
                var fileCount = Directory.GetFiles(currentFolderPath).Length;
                if (fileCount >= MaxFilesPerFolder)
                {
                    folderNumber++;
                }
            }

            var newFolderPath = Path.Combine(fileTypeFolderPath, folderNumber.ToString());
            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
            }

            return newFolderPath;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return await Task.FromResult(false);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation($"File {filePath} deleted successfully");
                    return await Task.FromResult(true);
                }

                _logger.LogWarning($"File {filePath} not found for deletion");
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {filePath}");
                return await Task.FromResult(false);
            }
        }


        public async Task<byte[]?> GetFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _logger.LogWarning($"File {filePath} not found");
                    return null;
                }

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading file {filePath}");
                return null;
            }
        }

        // Sanitize filename to prevent path traversal attacks
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            // Remove any invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Further sanitize by removing potentially dangerous patterns
            sanitized = sanitized.Replace("..", string.Empty);
            
            return sanitized;
        }
    }
}