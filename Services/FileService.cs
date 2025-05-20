using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TE_Project.Enums;
using TE_Project.Helpers;
using TE_Project.Services.Interfaces;
using TE_Project.Repositories.Interfaces;

namespace TE_Project.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IPlantRepository _plantRepository;
        private const int MaxFilesPerFolder = 100;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        
        private readonly string _customUploadPath = @"C:\Users\youne\OneDrive\Bureau\test";

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger, IPlantRepository plantRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
            
            if (!Directory.Exists(_customUploadPath))
            {
                Directory.CreateDirectory(_customUploadPath);
            }
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

                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    throw new ArgumentException($"File extension {extension} is not allowed.");
                }

                var folderPath = await GetNewFolderPath(plantId, fileType);
                
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                
                if (File.Exists(filePath))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    fileName = $"{fileNameWithoutExt}_{Guid.NewGuid():N}{extension}";
                    filePath = Path.Combine(folderPath, fileName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File {FileName} saved successfully to {FolderPath}", fileName, folderPath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {FileName} for plant {PlantId}, file type {FileType}", fileName, plantId, fileType);
                throw new InvalidOperationException($"Error occurred while saving the file: {ex.Message}", ex);
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

                if (!FileValidationHelper.IsValidFileName(originalFileName, fileType))
                {
                    throw new ArgumentException($"File name does not match the pattern for {fileType}");
                }

                var sanitizedName = SanitizeFileName(nameWithoutExtension);
                
                return $"{sanitizedName}{extension}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file name {OriginalFileName} for file type {FileType}", originalFileName, fileType);
                throw new InvalidOperationException($"Error occurred while processing the file name '{originalFileName}' for file type '{fileType}': {ex.Message}", ex);
            }
        }

        public async Task<string> GetNewFolderPath(int plantId, FileType fileType)
        {
            if (plantId <= 0)
            {
                throw new ArgumentException("Invalid plant ID");
            }
            
            var plant = await _plantRepository.GetByIdAsync(plantId);
            if (plant == null)
            {
                throw new ArgumentException($"Plant with ID {plantId} not found");
            }
            
            var sanitizedPlantName = SanitizeFileName(plant.Name);
            
            var baseUploadFolder = _customUploadPath;

            var plantFolderName = $"{sanitizedPlantName}";
            var plantFolderPath = Path.Combine(baseUploadFolder, plantFolderName);
            if (!Directory.Exists(plantFolderPath))
            {
                Directory.CreateDirectory(plantFolderPath);
            }

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

            var folders = Directory.GetDirectories(fileTypeFolderPath)
                .Select(d => new DirectoryInfo(d).Name)
                .Where(name => int.TryParse(name, out _))
                .Select(int.Parse)
                .OrderBy(num => num)
                .ToList();

            int folderNumber = 1;
            if (folders.Any())
            {
                folderNumber = folders[folders.Count - 1];
                var currentFolderPath = Path.Combine(fileTypeFolderPath, folderNumber.ToString());

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
                    _logger.LogInformation("File {FilePath} deleted successfully", filePath);
                    return await Task.FromResult(true);
                }

                _logger.LogWarning("File {FilePath} not found for deletion", filePath);
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
                return await Task.FromResult(false);
            }
        }


        public async Task<byte[]?> GetFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _logger.LogWarning("File {FilePath} not found", filePath);
                    return null;
                }

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file {FilePath}", filePath);
                return null;
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            
            sanitized = sanitized.Replace("..", string.Empty);
            
            sanitized = sanitized.Replace(" ", "_");
            
            return sanitized;
        }
    }
}