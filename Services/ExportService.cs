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
        private readonly ILogger<ExportService> _logger;

        public ExportService(
            ISubmissionRepository submissionRepository,
            ILogger<ExportService> logger)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
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
                if (plantId.HasValue)
                {
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
                    1 => GenerateFormat1Report(submissions.ToList()),
                    2 => GenerateFormat2Report(submissions.ToList()),
                    _ => throw new ArgumentException("Invalid report format")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw new InvalidOperationException("Failed to generate report. Please try again later.", ex);
            }
        }

        private FileContentResult GenerateFormat1Report(List<Submission> submissions)
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
                
                // Headers
                worksheet.Cell(1, 1).Value = "Last Name";
                worksheet.Cell(1, 2).Value = "First Name";
                worksheet.Cell(1, 3).Value = "Gender";
                worksheet.Cell(1, 4).Value = "National ID";
                worksheet.Cell(1, 5).Value = "Date of Birth";
                worksheet.Cell(1, 6).Value = "Plant";

                // Make headers bold
                worksheet.Range(1, 1, 1, 6).Style.Font.Bold = true;

                // Data rows
                for (int i = 0; i < batch.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = batch[i].LastName;
                    worksheet.Cell(i + 2, 2).Value = batch[i].FirstName;
                    worksheet.Cell(i + 2, 3).Value = batch[i].Gender.ToString();
                    worksheet.Cell(i + 2, 4).Value = batch[i].Cin;
                    worksheet.Cell(i + 2, 5).Value = batch[i].DateOfBirth.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 6).Value = batch[i].Plant?.Name ?? $"Plant {batch[i].PlantId}";
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

            return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Report_Format1_{DateTime.Now:yyyyMMdd}.xlsx"
            };
        }

        private FileContentResult GenerateFormat2Report(List<Submission> submissions)
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
                
                // Headers
                worksheet.Cell(1, 1).Value = "National ID";
                worksheet.Cell(1, 2).Value = "Registration Number";
                worksheet.Cell(1, 3).Value = "Plant";

                // Make headers bold
                worksheet.Range(1, 1, 1, 3).Style.Font.Bold = true;

                // Data rows
                for (int i = 0; i < batch.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = batch[i].Cin;
                    worksheet.Cell(i + 2, 2).Value = batch[i].GreyCard;
                    worksheet.Cell(i + 2, 3).Value = batch[i].Plant?.Name ?? $"Plant {batch[i].PlantId}";
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

            return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Report_Format2_{DateTime.Now:yyyyMMdd}.xlsx"
            };
        }
    }
}