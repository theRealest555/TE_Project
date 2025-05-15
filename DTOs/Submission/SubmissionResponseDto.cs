using TE_Project.Enums;

namespace TE_Project.DTOs.Submission
{
    /// <summary>
    /// Data transfer object for submission response
    /// </summary>
    public class SubmissionResponseDto
    {
        /// <summary>
        /// Submission ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// First name of the person
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Last name of the person
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gender of the person
        /// </summary>
        public GenderType Gender { get; set; }
        
        /// <summary>
        /// TE ID of the person
        /// </summary>
        public string TeId { get; set; } = string.Empty;
        
        /// <summary>
        /// CIN number
        /// </summary>
        public string Cin { get; set; } = string.Empty;
        
        /// <summary>
        /// Date of birth
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        
        /// <summary>
        /// Grey card number (if available)
        /// </summary>
        public string? GreyCard { get; set; }
        
        /// <summary>
        /// Plant ID
        /// </summary>
        public int PlantId { get; set; }
        
        /// <summary>
        /// Plant name
        /// </summary>
        public string? PlantName { get; set; }
        
        /// <summary>
        /// Date when the submission was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// List of files associated with the submission
        /// </summary>
        public List<FileDto> Files { get; set; } = new List<FileDto>();
    }

    /// <summary>
    /// Data transfer object for file information
    /// </summary>
    public class FileDto
    {
        /// <summary>
        /// File ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of file
        /// </summary>
        public FileType FileType { get; set; }
        
        /// <summary>
        /// File type as string description
        /// </summary>
        public string FileTypeDescription => FileTypeToString(FileType);
        
        /// <summary>
        /// Date when the file was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Converts a FileType enum value to a descriptive string
        /// </summary>
        private static string FileTypeToString(FileType fileType)
        {
            return fileType switch
            {
                FileType.Cin => "CIN Document",
                FileType.PIC => "Personal Photo",
                FileType.CG => "Grey Card",
                _ => "Unknown"
            };
        }
    }
}