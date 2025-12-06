using SmartClass.Domain.Enums;

namespace SmartClass.Domain.Entities
{
    public class Submission : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.NotSubmitted;
        public DateTime? SubmittedAt { get; set; }
    }
}
