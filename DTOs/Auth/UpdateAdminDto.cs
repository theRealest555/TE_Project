using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for updating an admin user
    /// </summary>
    public class UpdateAdminDto
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
        public string? TEID { get; set; }

        /// <summary>
        /// Email address of the admin user
        /// </summary>
        /// <example>john.doe@example.com</example>
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Plant ID where the admin user belongs
        /// </summary>
        /// <example>1</example>
        [Range(1, 5, ErrorMessage = "Please select a valid plant (1-5).")]
        public int PlantId { get; set; }

        /// <summary>
        /// Flag to indicate if the user is a super admin
        /// </summary>
        /// <example>false</example>
        public bool? IsSuperAdmin { get; set; }
    }
}