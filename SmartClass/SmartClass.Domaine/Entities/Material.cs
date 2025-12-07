using SmartClass.Domain.Entities;

public class Material : BaseEntity
{
    public Guid ClassroomId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContentHtml { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // 🔹 НОВЕ: всі завдання по цій темі
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    // 🔹 НОВЕ: файли, прикріплені до матеріалу
    public ICollection<FileResource> Files { get; set; } = new List<FileResource>();
}
