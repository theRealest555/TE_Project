using System.ComponentModel.DataAnnotations;

namespace TE_Project.Entities
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TeId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Cin { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(50)]
        public string GreyCard { get; set; } = string.Empty;

        [Required]
        public int PlantId { get; set; }
        public Plant Plant { get; set; } = null!;

        public ICollection<UploadedFile> Files { get; set; } = new List<UploadedFile>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}