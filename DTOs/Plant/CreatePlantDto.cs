using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Plant
{
    /// <summary>
    /// Data transfer object for creating a new plant
    /// </summary>
    public class CreatePlantDto
    {
        /// <summary>
        /// Name of the plant
        /// </summary>
        /// <example>Plant Alpha</example>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the plant
        /// </summary>
        /// <example>Main manufacturing facility located in the north sector</example>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}