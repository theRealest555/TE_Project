using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Plant
{
    public class CreatePlantDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}