using MediatR;

namespace SmartClass.Application.Common.DomainEvents;

public sealed class GradeGivenEvent : INotification
{
    public Guid AssignmentId { get; }
    public Guid SubmissionId { get; }
    public Guid StudentId { get; }
    public Guid GradedBy { get; }
    public decimal Score { get; }

    public GradeGivenEvent(Guid assignmentId, Guid submissionId, Guid studentId, Guid gradedBy, decimal score)
    {
        AssignmentId = assignmentId;
        SubmissionId = submissionId;
        StudentId = studentId;
        GradedBy = gradedBy;
        Score = score;
    }
}
