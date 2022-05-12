namespace FTPUpload.Models
{
    internal class Document
    {
        public string Filename { get; set; } = string.Empty;

        public DateTimeOffset UploadedDate { get; set; }
    }
}
