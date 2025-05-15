using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using TE_Project.DTOs.Export;
using TE_Project.Entities;
using TE_Project.Enums;
using TE_Project.Repositories.Interfaces;
using TE_Project.Services.Interfaces;

namespace TE_Project.Services
{
    public class ExportService : IExportService
    {
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly ILogger<ExportService> _logger;

        public ExportService(
            ISubmissionRepository submissionRepository,
            IPlantRepository plantRepository,
            ILogger<ExportService> logger)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FileContentResult> GenerateExcelReportAsync(ExportDto exportDto, int? adminPlantId, bool isSuperAdmin)
        {
            try
            {
                var plantId = exportDto.PlantId ?? adminPlantId;

                // Enforce plant restriction for regular admins
                if (!isSuperAdmin && plantId != adminPlantId)
                {
                    plantId = adminPlantId;
                }

                // Fetch submissions based on plant ID
                IEnumerable<Submission> submissions;
                string? plantName = null;
                
                if (plantId.HasValue)
                {
                    // Get the plant name for the filename
                    var plant = await _plantRepository.GetByIdAsync(plantId.Value);
                    if (plant != null)
                    {
                        plantName = plant.Name;
                    }
                    
                    submissions = await _submissionRepository.GetWithFilesByPlantIdAsync(plantId.Value);
                }
                else if (isSuperAdmin)
                {
                    submissions = await _submissionRepository.GetAllAsync(includeProperties: "Files,Plant");
                }
                else
                {
                    throw new UnauthorizedAccessException("Access denied to export data");
                }

                // Order by creation date (oldest first)
                submissions = submissions.OrderBy(s => s.CreatedAt).ToList();

                return exportDto.Format switch
                {
                    1 => GenerateFormat1Report(submissions.ToList(), plantId, plantName),
                    2 => GenerateFormat2Report(submissions.ToList(), plantId, plantName),
                    _ => throw new ArgumentException("Invalid report format")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw new InvalidOperationException("Failed to generate report. Please try again later.", ex);
            }
        }

        private FileContentResult GenerateFormat1Report(List<Submission> submissions, int? plantId = null, string? plantName = null)
        {
            using var workbook = new XLWorkbook();
            
            // Group submissions into batches of 100
            var batchedSubmissions = submissions
                .Select((s, index) => new { Submission = s, Index = index })
                .GroupBy(x => x.Index / 100)
                .Select(g => g.Select(x => x.Submission).ToList())
                .ToList();
            
            // Create at least one sheet even if there are no submissions
            if (batchedSubmissions.Count == 0)
            {
                batchedSubmissions.Add(new List<Submission>());
            }
            
            int sheetNumber = 1;
            
            foreach (var batch in batchedSubmissions)
            {
                var worksheet = workbook.Worksheets.Add($"Report_{sheetNumber}");
                
                // Headers - Updated according to Image 2
                worksheet.Cell(1, 1).Value = "Last name";
                worksheet.Cell(1, 2).Value = "First name";
                worksheet.Cell(1, 3).Value = "Gender";
                worksheet.Cell(1, 4).Value = "National ID";
                worksheet.Cell(1, 5).Value = "Date of birth";

                // Make headers bold
                worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;

                // Data rows
                for (int i = 0; i < batch.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = batch[i].LastName;
                    worksheet.Cell(i + 2, 2).Value = batch[i].FirstName;
                    worksheet.Cell(i + 2, 3).Value = batch[i].Gender.ToString();
                    worksheet.Cell(i + 2, 4).Value = batch[i].Cin;
                    worksheet.Cell(i + 2, 5).Value = batch[i].DateOfBirth.ToString("yyyy-MM-dd");
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
                
                sheetNumber++;
            }

            // Convert to bytes
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var content = stream.ToArray();

            // Create file name with plant information
            string plantInfo = plantId.HasValue 
                ? (!string.IsNullOrEmpty(plantName) ? $"_{plantName}" : $"_Plant{plantId}")
                : "_AllPlants";
                
            // Sanitize the plant name for file naming
            if (plantInfo != "_AllPlants")
            {
                plantInfo = plantInfo.Replace(" ", "_").Replace("/", "_").Replace("\\", "_")
                                  .Replace(":", "_").Replace("*", "_").Replace("?", "_")
                                  .Replace("\"", "_").Replace("<", "_").Replace(">", "_")
                                  .Replace("|", "_");
            }

            return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Report_Format1{plantInfo}_{DateTime.Now:yyyyMMdd}.xlsx"
            };
        }

        private FileContentResult GenerateFormat2Report(List<Submission> submissions, int? plantId = null, string? plantName = null)
        {
            using var workbook = new XLWorkbook();
            
            // Filter submissions with grey card
            var submissionsWithGreyCard = submissions
                .Where(s => !string.IsNullOrEmpty(s.GreyCard))
                .ToList();
            
            // Group submissions into batches of 100
            var batchedSubmissions = submissionsWithGreyCard
                .Select((s, index) => new { Submission = s, Index = index })
                .GroupBy(x => x.Index / 100)
                .Select(g => g.Select(x => x.Submission).ToList())
                .ToList();
            
            // Create at least one sheet even if there are no submissions
            if (batchedSubmissions.Count == 0)
            {
                batchedSubmissions.Add(new List<Submission>());
            }
            
            int sheetNumber = 1;
            
            foreach (var batch in batchedSubmissions)
            {
                var worksheet = workbook.Worksheets.Add($"Report_{sheetNumber}");
                
                // Headers - Updated according to Image 1
                worksheet.Cell(1, 1).Value = "National ID";
                worksheet.Cell(1, 2).Value = "Registration number";

                // Make headers bold
                worksheet.Range(1, 1, 1, 2).Style.Font.Bold = true;

                // Data rows
                for (int i = 0; i < batch.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = batch[i].Cin;
                    worksheet.Cell(i + 2, 2).Value = batch[i].GreyCard;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
                
                sheetNumber++;
            }

            // Convert to bytes
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var content = stream.ToArray();

            // Create file name with plant information
            string plantInfo = plantId.HasValue 
                ? (!string.IsNullOrEmpty(plantName) ? $"_{plantName}" : $"_Plant{plantId}")
                : "_AllPlants";
                
            // Sanitize the plant name for file naming
            if (plantInfo != "_AllPlants")
            {
                plantInfo = plantInfo.Replace(" ", "_").Replace("/", "_").Replace("\\", "_")
                                  .Replace(":", "_").Replace("*", "_").Replace("?", "_")
                                  .Replace("\"", "_").Replace("<", "_").Replace(">", "_")
                                  .Replace("|", "_");
            }

            return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Report_Format2{plantInfo}_{DateTime.Now:yyyyMMdd}.xlsx"
            };
        }
    }
}