namespace SmartClass.Domain.Entities
{
    public class Assignment : BaseEntity
    {
        public Guid ClassroomId { get; set; }
        public Guid CreatedBy { get; set; }

        // Опційний звʼязок з темою (Material)
        public Guid? MaterialId { get; set; }
        public Material? Material { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? DescriptionHtml { get; set; }
        public int PointsMax { get; set; } = 100;
        public DateTime? DueAt { get; set; }
        public bool AllowLate { get; set; }
        public string Status { get; set; } = "Draft";   // Draft|Published|Closed

        // Файли, прикріплені до завдання
        public ICollection<FileResource> Files { get; set; } = new List<FileResource>();

        // Відповіді студентів
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
