using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Plant
{
    /// <summary>
    /// Data transfer object for updating a plant
    /// </summary>
    public class UpdatePlantDto
    {
        /// <summary>
        /// New name of the plant
        /// </summary>
        /// <example>Plant Beta</example>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// New description of the plant
        /// </summary>
        /// <example>Updated manufacturing facility with expanded capabilities</example>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}