using System.ComponentModel.DataAnnotations;

namespace TE_Project.DTOs.Auth
{
    public class ChangePasswordDto
    {
        /// <summary>
        /// Current password of the user
        /// </summary>
        /// <example>OldPassword123!</example>
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password for the user
        /// </summary>
        /// <example>NewPassword456!</example>
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of the new password
        /// </summary>
        /// <example>NewPassword456!</example>
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}