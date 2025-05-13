using System.ComponentModel.DataAnnotations;

namespace TE_Project.Entities
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        [MaxLength(100)]
        public string? DeviceInfo { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }
    }
}