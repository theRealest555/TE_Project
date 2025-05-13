namespace TE_Project.DTOs.Export
{
    public class ExportDto
    {
        /// <summary>
        /// Format of the export: 1 for all submissions, 2 for submissions with grey cards
        /// </summary>
        public int Format { get; set; }
        
        /// <summary>
        /// Optional plant ID - if null and user is super admin, export all plants
        /// </summary>
        public int? PlantId { get; set; }
    }
}