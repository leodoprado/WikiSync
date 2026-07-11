namespace WikiSync.Contracts
{
    public class UploadFileRequest
    {
        public string RelativePath { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public DateTime LastModifiedAt { get; set; }
    }
}
