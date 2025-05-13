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
                throw new ApplicationException("Failed to generate report. Please try again later.", ex);
            }
        }

        private FileContentResult GenerateFormat1Report(List<Submission> submissions)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            // Headers
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "CIN";
            worksheet.Cell(1, 3).Value = "TE ID";
            worksheet.Cell(1, 4).Value = "Date of Birth";
            worksheet.Cell(1, 5).Value = "Plant";

            // Make headers bold
            worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;

            // Data rows
            for (int i = 0; i < submissions.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = submissions[i].FullName;
                worksheet.Cell(i + 2, 2).Value = submissions[i].Cin;
                worksheet.Cell(i + 2, 3).Value = submissions[i].TeId;
                worksheet.Cell(i + 2, 4).Value = submissions[i].DateOfBirth.ToString("yyyy-MM-dd");
                worksheet.Cell(i + 2, 5).Value = submissions[i].Plant?.Name ?? $"Plant {submissions[i].PlantId}";
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

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
            var worksheet = workbook.Worksheets.Add("Report");

            // Headers
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Vehicle Registration Number";
            worksheet.Cell(1, 3).Value = "TE ID";
            worksheet.Cell(1, 4).Value = "Plant";

            // Make headers bold
            worksheet.Range(1, 1, 1, 4).Style.Font.Bold = true;

            // Data rows
            int row = 2;
            foreach (var submission in submissions)
            {
                if (!string.IsNullOrEmpty(submission.GreyCard))
                {
                    worksheet.Cell(row, 1).Value = submission.FullName;
                    worksheet.Cell(row, 2).Value = submission.GreyCard;
                    worksheet.Cell(row, 3).Value = submission.TeId;
                    worksheet.Cell(row, 4).Value = submission.Plant?.Name ?? $"Plant {submission.PlantId}";
                    row++;
                }
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

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