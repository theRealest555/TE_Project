using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TE_Project.Enums;

namespace TE_Project.DTOs.Submission
{
    public class SubmissionDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public GenderType Gender { get; set; }

        [Required]
        [MaxLength(50)]
        public string TeId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]{1,2}[0-9]+$", ErrorMessage = "CIN format is invalid.")]
        public string Cin { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Please select a valid plant (1-5).")]
        public int PlantId { get; set; }

        [RegularExpression(@"^[0-9]+-[A-Za-z]-[0-9]+$", ErrorMessage = "Grey card number format is invalid.")]
        public string GreyCard { get; set; } = string.Empty;

        [Required]
        public IFormFile CinImage { get; set; } = null!;

        [Required]
        public IFormFile PicImage { get; set; } = null!;

        public IFormFile? GreyCardImage { get; set; }
    }
}