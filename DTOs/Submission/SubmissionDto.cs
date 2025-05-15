using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TE_Project.Enums;

namespace TE_Project.DTOs.Submission
{
    /// <summary>
    /// Data transfer object for creating a submission
    /// </summary>
    public class SubmissionDto
    {
        /// <summary>
        /// First name of the person
        /// </summary>
        /// <example>John</example>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name of the person
        /// </summary>
        /// <example>Smith</example>
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gender of the person
        /// </summary>
        /// <example>Male</example>
        [Required]
        public GenderType Gender { get; set; }

        /// <summary>
        /// TE ID of the person
        /// </summary>
        /// <example>TE123456</example>
        [Required]
        [MaxLength(50)]
        public string TeId { get; set; } = string.Empty;

        /// <summary>
        /// CIN number
        /// </summary>
        /// <example>AB123456</example>
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]{1,2}[0-9]+$", ErrorMessage = "CIN format is invalid.")]
        public string Cin { get; set; } = string.Empty;

        /// <summary>
        /// Date of birth
        /// </summary>
        /// <example>1990-01-01</example>
        [Required]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Plant ID
        /// </summary>
        /// <example>1</example>
        [Required]
        [Range(1, 5, ErrorMessage = "Please select a valid plant (1-5).")]
        public int PlantId { get; set; }

        /// <summary>
        /// Grey card number (optional)
        /// </summary>
        /// <example>12345-A-67890</example>
        [RegularExpression(@"^[0-9]+-[A-Za-z]-[0-9]+$", ErrorMessage = "Grey card number format is invalid.")]
        public string GreyCard { get; set; } = string.Empty;

        /// <summary>
        /// CIN image file
        /// </summary>
        [Required]
        public IFormFile CinImage { get; set; } = null!;

        /// <summary>
        /// Personal photo image file
        /// </summary>
        [Required]
        public IFormFile PicImage { get; set; } = null!;

        /// <summary>
        /// Grey card image file (optional)
        /// </summary>
        public IFormFile? GreyCardImage { get; set; }
    }
}