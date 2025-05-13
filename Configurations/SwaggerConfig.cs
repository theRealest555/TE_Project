namespace TE_Project.Configurations
{
    public class SwaggerConfig
    {
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ContactInfo Contact { get; set; } = new ContactInfo();
        public bool EnableXmlComments { get; set; }
    }

    public class ContactInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}