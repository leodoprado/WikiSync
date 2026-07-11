namespace WikiSync.Models
{
    public class FileChange
    {
        public FileChangeType ChangeType { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string? OldFilePath { get; set; }
    }
}
