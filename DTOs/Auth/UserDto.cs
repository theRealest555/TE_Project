namespace TE_Project.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for user profile information
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Plant ID where the user belongs
        /// </summary>
        public int PlantId { get; set; }
        
        /// <summary>
        /// Name of the plant
        /// </summary>
        public string? PlantName { get; set; }
        
        /// <summary>
        /// Flag indicating if the user is a super admin
        /// </summary>
        public bool IsSuperAdmin { get; set; }
        
        /// <summary>
        /// Flag indicating if the user needs to change their password
        /// </summary>
        public bool RequirePasswordChange { get; set; }
        
        /// <summary>
        /// List of roles assigned to the user
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
}