using MediatR;

namespace SmartClass.Application.Common.DomainEvents;

public sealed class AssignmentCreatedEvent : INotification
{
    public Guid ClassroomId { get; }
    public Guid AssignmentId { get; }
    public Guid CreatedBy { get; }
    public string Title { get; }

    public AssignmentCreatedEvent(Guid classroomId, Guid assignmentId, Guid createdBy, string title)
    {
        ClassroomId = classroomId;
        AssignmentId = assignmentId;
        CreatedBy = createdBy;
        Title = title;
    }
}
