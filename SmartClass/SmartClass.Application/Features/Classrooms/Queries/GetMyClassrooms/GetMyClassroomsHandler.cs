using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;

namespace SmartClass.Application.Features.Classrooms.Queries.GetMyClassrooms;

public sealed class GetMyClassroomsHandler
    : IRequestHandler<GetMyClassroomsQuery, IReadOnlyList<ClassroomDto>>
{
    private readonly IRepository<Classroom> classroomRepository;
    private readonly IRepository<ClassMember> memberRepository;
    private readonly ICurrentUser currentUser;

    public GetMyClassroomsHandler(
        IRepository<Classroom> classroomRepository,
        IRepository<ClassMember> memberRepository,
        ICurrentUser currentUser)
    {
        this.classroomRepository = classroomRepository;
        this.memberRepository = memberRepository;
        this.currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ClassroomDto>> Handle(
        GetMyClassroomsQuery request,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        // отримуємо класи, створені викладачем
        var teacherClasses = await classroomRepository.GetListAsync(
            x => x.OwnerId == userId && !x.IsArchived);

        // отримуємо класи, у яких користувач студент
        var memberLinks = await memberRepository.GetListAsync(
            x => x.UserId == userId);

        var memberClassIds = memberLinks.Select(m => m.ClassroomId).ToHashSet();

        var memberClasses = await classroomRepository.GetListAsync(
            x => memberClassIds.Contains(x.Id));

        // об’єднуємо два списки
        var allClasses = teacherClasses
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                Title = c.Title,
                Section = c.Section,
                JoinCode = c.JoinCode,
                Description = c.Description,
                IsArchived = c.IsArchived,
                RoleInClass = "Teacher"
            })
            .Concat(memberClasses.Select(c => new ClassroomDto
            {
                Id = c.Id,
                Title = c.Title,
                Section = c.Section,
                JoinCode = "", // студент не бачить код
                Description = c.Description,
                IsArchived = c.IsArchived,
                RoleInClass = "Student"
            }))
            .ToList();

        return allClasses;
    }
}
