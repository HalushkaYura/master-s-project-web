using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums; // для ChannelType, ClassRole

namespace SmartClass.Application.Features.Assignments.Commands;

public sealed class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IRepository<Assignment> assignmentRepository;
    private readonly IRepository<Channel> channelRepository;
    private readonly IRepository<ClassMember> classMemberRepository;
    private readonly IRepository<ChannelMember> channelMemberRepository;
    private readonly ICurrentUserService currentUser;
    private readonly IMediator mediator;

    public CreateAssignmentHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<Channel> channelRepository,
        IRepository<ClassMember> classMemberRepository,
        IRepository<ChannelMember> channelMemberRepository,
        ICurrentUserService currentUser,
        IMediator mediator)
    {
        this.assignmentRepository = assignmentRepository;
        this.channelRepository = channelRepository;
        this.classMemberRepository = classMemberRepository;
        this.channelMemberRepository = channelMemberRepository;
        this.currentUser = currentUser;
        this.mediator = mediator;
    }

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        // 1) Створюємо завдання
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            ClassroomId = request.ClassroomId,
            CreatedBy = userId,
            Title = request.Title,
            DescriptionHtml = request.DescriptionHtml,
            PointsMax = request.PointsMax,
            DueAt = request.DueAt,
            AllowLate = request.AllowLate,
            Status = "Draft" // або "Published", як домовишся
        };
        await assignmentRepository.AddAsync(assignment);

        // 2) Створюємо канал обговорення завдання
        var discussionChannel = new Channel
        {
            Id = Guid.NewGuid(),
            ClassroomId = request.ClassroomId,
            Title = $"Discussion: {request.Title}",
            Type = ChannelType.Assignment // або свій тип
        };
        await channelRepository.AddAsync(discussionChannel);

        // 3) Додаємо всіх учасників класу в цей канал
        var members = await classMemberRepository
            .GetListAsync(m => m.ClassroomId == request.ClassroomId);

        foreach (var member in members)
        {
            var cm = new ChannelMember
            {
                Id = Guid.NewGuid(),
                ChannelId = discussionChannel.Id,
                UserId = member.UserId,
                IsMuted = false
            };
            await channelMemberRepository.AddAsync(cm);
        }

        // 4) Один SaveChanges на всі зміни
        await assignmentRepository.SaveChangesAsync();

        // 5) Публікуємо подію для нотифікацій
        await mediator.Publish(new AssignmentCreatedEvent(
                classroomId: assignment.ClassroomId,
                assignmentId: assignment.Id,
                createdBy: userId,
                title: assignment.Title
            ), ct);

        return assignment.Id;
    }
}
