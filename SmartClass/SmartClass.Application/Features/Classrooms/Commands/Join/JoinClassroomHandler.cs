using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Features.Classrooms.Specifications;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;

namespace SmartClass.Application.Features.Classrooms.Commands.Join;

public sealed class JoinClassroomHandler : IRequestHandler<JoinClassroomCommand, Guid>
{
    private readonly IRepository<Classroom> classroomRepository;
    private readonly IRepository<ClassMember> memberRepository;
    private readonly ICurrentUser currentUser;

    public JoinClassroomHandler(
        IRepository<Classroom> classroomRepository,
        IRepository<ClassMember> memberRepository,
        ICurrentUser currentUser)
    {
        this.classroomRepository = classroomRepository;
        this.memberRepository = memberRepository;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(JoinClassroomCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var classroom = await classroomRepository.GetFirstBySpecAsync(new ClassroomByJoinCodeSpec(request.JoinCode));

        if (classroom is null)
            throw new InvalidOperationException("Invalid join code.");

        var alreadyMember = (await memberRepository.GetListAsync(
            x => x.ClassroomId == classroom.Id && x.UserId == userId)).Any();

        if (alreadyMember)
            throw new InvalidOperationException("User is already a member of this classroom.");

        var member = new ClassMember
        {
            ClassroomId = classroom.Id,
            UserId = userId,
            RoleInClass = ClassRole.Student
        };

        await memberRepository.AddAsync(member);
        await memberRepository.SaveChangesAsync();

        return classroom.Id;
    }
}
