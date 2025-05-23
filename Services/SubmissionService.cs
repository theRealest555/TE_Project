using Microsoft.Extensions.Logging;
using TE_Project.DTOs.Submission;
using TE_Project.Entities;
using TE_Project.Enums;
using TE_Project.Repositories.Interfaces;
using TE_Project.Services.Interfaces;

namespace TE_Project.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(
            ISubmissionRepository submissionRepository,
            IPlantRepository plantRepository,
            IFileService fileService,
            ILogger<SubmissionService> logger)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Submission> CreateSubmissionAsync(SubmissionDto model)
        {
            var plant = await _plantRepository.GetByIdAsync(model.PlantId);
            if (plant == null)
            {
                throw new ArgumentException("Invalid plant");
            }

            var existingSubmission = await _submissionRepository.GetFirstOrDefaultAsync(s => s.Cin == model.Cin);
            if (existingSubmission != null)
            {
                throw new ArgumentException("A submission with this CIN already exists");
            }

            if (!ValidateFiles(model))
            {
                throw new ArgumentException("Invalid file(s). Files must be images (jpg, jpeg, png) and less than 1MB.");
            }

            var submission = new Submission
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                TeId = model.TeId,
                Cin = model.Cin,
                DateOfBirth = model.DateOfBirth,
                PlantId = model.PlantId,
                GreyCard = model.GreyCard,
                Files = new List<UploadedFile>()
            };

            await _submissionRepository.AddAsync(submission);
            await _submissionRepository.SaveChangesAsync();

            try
            {
                var cinFileName = _fileService.GetFileNameForSaving(model.Cin + Path.GetExtension(model.CinImage.FileName), FileType.Cin);
                var cinFilePath = await _fileService.SaveFileAsync(model.CinImage, model.PlantId, FileType.Cin, cinFileName);
                submission.Files.Add(new UploadedFile
                {
                    FileName = cinFileName,
                    FilePath = cinFilePath,
                    FileType = FileType.Cin,
                    SubmissionId = submission.Id
                });

                var picFileName = _fileService.GetFileNameForSaving(model.Cin + "_i" + Path.GetExtension(model.PicImage.FileName), FileType.PIC);
                var picFilePath = await _fileService.SaveFileAsync(model.PicImage, model.PlantId, FileType.PIC, picFileName);
                submission.Files.Add(new UploadedFile
                {
                    FileName = picFileName,
                    FilePath = picFilePath,
                    FileType = FileType.PIC,
                    SubmissionId = submission.Id
                });

                if (model.GreyCardImage != null && !string.IsNullOrEmpty(model.GreyCard))
                {
                    var greyCardFileName = _fileService.GetFileNameForSaving(model.GreyCard + Path.GetExtension(model.GreyCardImage.FileName), FileType.CG);
                    var greyCardFilePath = await _fileService.SaveFileAsync(model.GreyCardImage, model.PlantId, FileType.CG, greyCardFileName);
                    submission.Files.Add(new UploadedFile
                    {
                        FileName = greyCardFileName,
                        FilePath = greyCardFilePath,
                        FileType = FileType.CG,
                        SubmissionId = submission.Id
                    });
                }

                await _submissionRepository.SaveChangesAsync();
                
                _logger.LogInformation("Created submission {SubmissionId} for plant {PlantId}", submission.Id, model.PlantId);
                return submission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving files for submission {SubmissionId}", submission.Id);
                
                await DeleteSubmissionAsync(submission);
                
                throw new InvalidOperationException($"Error saving files: {ex.Message}", ex);
            }
        }

        public async Task<SubmissionResponseDto?> GetSubmissionByCinAsync(string cin)
        {
            var submission = await _submissionRepository.GetFirstOrDefaultAsync(
                s => s.Cin == cin,
                includeProperties: "Plant,Files");
            
            if (submission == null)
                return null;

            return MapToSubmissionResponseDto(submission);
        }

        public async Task<SubmissionResponseDto?> GetSubmissionByIdAsync(int id)
        {
            var submission = await _submissionRepository.GetWithFilesAsync(id);
            
            if (submission == null)
                return null;

            return MapToSubmissionResponseDto(submission);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByPlantIdAsync(int plantId)
        {
            var submissions = await _submissionRepository.GetWithFilesByPlantIdAsync(plantId);
            return submissions.Select(MapToSubmissionResponseDto);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetAllSubmissionsAsync()
        {
            var submissions = await _submissionRepository.GetAllAsync(
                includeProperties: "Files,Plant",
                trackChanges: false);
                
            return submissions.Select(MapToSubmissionResponseDto);
        }

        private SubmissionResponseDto MapToSubmissionResponseDto(Submission submission)
        {
            return new SubmissionResponseDto
            {
                Id = submission.Id,
                FirstName = submission.FirstName,
                LastName = submission.LastName,
                Gender = submission.Gender,
                TeId = submission.TeId,
                Cin = submission.Cin,
                DateOfBirth = submission.DateOfBirth,
                GreyCard = submission.GreyCard,
                PlantId = submission.PlantId,
                PlantName = submission.Plant?.Name,
                CreatedAt = submission.CreatedAt,
                Files = submission.Files.Select(f => new FileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    UploadedAt = f.UploadedAt
                }).ToList()
            };
        }

        private static bool ValidateFiles(SubmissionDto model)
        {
            // Check CIN image
            if (model.CinImage == null || model.CinImage.Length == 0 || model.CinImage.Length > 1024 * 1024) // 1MB
                return false;

            // Check PIC image
            if (model.PicImage == null || model.PicImage.Length == 0 || model.PicImage.Length > 1024 * 1024) // 1MB
                return false;

            // Check Grey Card image if Grey Card number is provided
            if (!string.IsNullOrEmpty(model.GreyCard) && 
                (model.GreyCardImage == null || model.GreyCardImage.Length == 0 || model.GreyCardImage.Length > 1024 * 1024)) // 1MB
                return false;

            return true;
        }

        private async Task DeleteSubmissionAsync(Submission submission)
        {
            try
            {
                foreach (var file in submission.Files)
                {
                    await _fileService.DeleteFileAsync(file.FilePath);
                }

                _submissionRepository.Remove(submission);
                await _submissionRepository.SaveChangesAsync();
                
                _logger.LogInformation("Deleted submission {SubmissionId}", submission.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting submission {SubmissionId}", submission.Id);
            }
        }
    }
}