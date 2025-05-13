using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for user login
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email address of the user
        /// </summary>
        /// <example>admin@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password of the user
        /// </summary>
        /// <example>Password123!</example>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}