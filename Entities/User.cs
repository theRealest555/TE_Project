using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TE_Project.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Full name of the user
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Plant ID where the user belongs
        /// </summary>
        [Required]
        public int PlantId { get; set; }
        
        /// <summary>
        /// Navigation property for the associated plant
        /// </summary>
        public Plant Plant { get; set; } = null!;

        /// <summary>
        /// Flag indicating if the user is a super admin
        /// </summary>
        [Required]
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// TEID for plant admins
        /// </summary>
        [MaxLength(50)]
        public string? TEID { get; set; }

        /// <summary>
        /// Flag to check if this is the first login
        /// </summary>
        public bool RequirePasswordChange { get; set; } = false;

        /// <summary>
        /// Date when the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Date when the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// List of roles assigned to the user (not mapped to database)
        /// </summary>
        [NotMapped]
        public IList<string> Roles { get; set; } = new List<string>();
    }
}