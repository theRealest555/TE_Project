namespace TE_Project.Configurations
{
    public class JwtConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string ExpireDays { get; set; } = string.Empty;
    }
}