using System.ComponentModel.DataAnnotations;
using TE_Project.Enums;

namespace TE_Project.Entities
{
    public class UploadedFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public FileType FileType { get; set; }

        [Required]
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}