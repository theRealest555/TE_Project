using Microsoft.AspNetCore.Mvc;
using TE_Project.DTOs.Export;

namespace TE_Project.Services.Interfaces
{
    public interface IExportService
    {
        Task<FileContentResult> GenerateExcelReportAsync(ExportDto exportDto, int? adminPlantId, bool isSuperAdmin);
    }
}