using MediatR;

namespace SmartClass.Application.Features.Assignments.Commands;

public sealed class CreateAssignmentCommand : IRequest<Guid>
{
    public Guid ClassroomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? DescriptionHtml { get; set; }
    public int PointsMax { get; set; } = 100;
    public DateTime? DueAt { get; set; }
    public bool AllowLate { get; set; } = false;
}
