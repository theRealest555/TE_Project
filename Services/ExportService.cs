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

                if (!isSuperAdmin && plantId != adminPlantId)
                {
                    plantId = adminPlantId;
                }

                IEnumerable<Submission> submissions;
                string? plantName = null;
                
                if (plantId.HasValue)
                {
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
            
            var batchedSubmissions = submissions
                .Select((s, index) => new { Submission = s, Index = index })
                .GroupBy(x => x.Index / 100)
                .Select(g => g.Select(x => x.Submission).ToList())
                .ToList();
            
            if (batchedSubmissions.Count == 0)
            {
                batchedSubmissions.Add(new List<Submission>());
            }
            
            int sheetNumber = 1;
            
            foreach (var batch in batchedSubmissions)
            {
                var worksheet = workbook.Worksheets.Add($"Report_{sheetNumber}");
                
                worksheet.Cell(1, 1).Value = "Last name";
                worksheet.Cell(1, 2).Value = "First name";
                worksheet.Cell(1, 3).Value = "Gender";
                worksheet.Cell(1, 4).Value = "National ID";
                worksheet.Cell(1, 5).Value = "Date of birth";

                worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;

                for (int i = 0; i < batch.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = batch[i].LastName;
                    worksheet.Cell(i + 2, 2).Value = batch[i].FirstName;
                    worksheet.Cell(i + 2, 3).Value = batch[i].Gender.ToString();
                    worksheet.Cell(i + 2, 4).Value = batch[i].Cin;
                    worksheet.Cell(i + 2, 5).Value = batch[i].DateOfBirth.ToString("yyyy-MM-dd");
                }

                worksheet.Columns().AdjustToContents();
                
                sheetNumber++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var content = stream.ToArray();

            string plantInfo;
            if (plantId.HasValue)
            {
                plantInfo = !string.IsNullOrEmpty(plantName) ? $"_{plantName}" : $"_Plant{plantId}";
            }
            else
            {
                plantInfo = "_AllPlants";
            }
                
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
            
            var submissionsWithGreyCard = submissions
                .Where(s => !string.IsNullOrEmpty(s.GreyCard))
                .ToList();
            
            var batchedSubmissions = submissionsWithGreyCard
                .Select((s, index) => new { Submission = s, Index = index })
                .GroupBy(x => x.Index / 100)
                .Select(g => g.Select(x => x.Submission).ToList())
                .ToList();
            
            if (batchedSubmissions.Count == 0)
            {
                batchedSubmissions.Add(new List<Submission>());
            }
            
            int sheetNumber = 1;
            
            foreach (var batch in batchedSubmissions)
            {
                var worksheet = workbook.Worksheets.Add($"Report_{sheetNumber}");
                
                worksheet.Cell(1, 1).Value = "National ID";
                worksheet.Cell(1, 2).Value = "Registration number";

                worksheet.Range(1, 1, 1, 2).Style.Font.Bold = true;

                for (int i = 0; i < batch.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = batch[i].Cin;
                    worksheet.Cell(i + 2, 2).Value = batch[i].GreyCard;
                }

                worksheet.Columns().AdjustToContents();
                
                sheetNumber++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var content = stream.ToArray();

            string plantInfo;
            if (plantId.HasValue)
            {
                plantInfo = !string.IsNullOrEmpty(plantName) ? $"_{plantName}" : $"_Plant{plantId}";
            }
            else
            {
                plantInfo = "_AllPlants";
            }
                
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