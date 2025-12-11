namespace SmartClass.Application.Contracts
{
    public class FileResourceDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public long SizeBytes { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; } // Add this property to fix CS0117
    }
}
