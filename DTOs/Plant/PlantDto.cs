namespace TE_Project.DTOs.Plant
{
    public class PlantDto
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Plant name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Plant description
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}