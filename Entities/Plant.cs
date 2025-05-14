using System.ComponentModel.DataAnnotations;

namespace TE_Project.Entities
{
    public class Plant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<User> Admins { get; set; } = new List<User>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}