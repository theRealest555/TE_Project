namespace TE_Project.Configurations
{
    public class CorsConfig
    {
        public string AllowedOrigins { get; set; } = string.Empty;
        public string[] AllowedMethods { get; set; } = Array.Empty<string>();
        public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; }
    }
}