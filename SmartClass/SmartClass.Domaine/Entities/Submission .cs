using SmartClass.Domain.Enums;

namespace SmartClass.Domain.Entities
{
    public class Submission : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.NotSubmitted;

        public Assignment Assignment { get; set; } = null!;

        public Grade? Grade { get; set; }

        public ICollection<FileResource> Files { get; set; } = new List<FileResource>();
    }
}
