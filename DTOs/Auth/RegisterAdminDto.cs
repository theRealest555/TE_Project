using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for registering an admin user
    /// </summary>
    public class RegisterAdminDto
    {
        /// <summary>
        /// Full name of the admin user
        /// </summary>
        /// <example>John Doe</example>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// TE ID of the admin user
        /// </summary>
        /// <example>TE12345</example>
        [Required]
        public string TEID { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the admin user
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Optional password with flexible requirements
        /// If not provided, a strong password will be generated
        /// </summary>
        /// <example>StrongP@ss123</example>
        [StringLength(100, MinimumLength = 8)]
        public string? Password { get; set; }

        /// <summary>
        /// Plant ID where the admin user belongs
        /// </summary>
        /// <example>1</example>
        [Required]
        [Range(1, 5, ErrorMessage = "Please select a valid plant (1-5).")]
        public int PlantId { get; set; }

        /// <summary>
        /// Flag to indicate if the user is a super admin
        /// </summary>
        /// <example>false</example>
        public bool IsSuperAdmin { get; set; } = false;
    }
}