using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Features.Classrooms.Specifications;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums; // для ClassRole, ChannelType

namespace SmartClass.Application.Features.Classrooms.Commands.Create;

public sealed class CreateClassroomHandler : IRequestHandler<CreateClassroomCommand, Guid>
{
    private readonly IRepository<Classroom> classroomRepository;
    private readonly IRepository<ClassMember> classMemberRepository;
    private readonly IRepository<Channel> channelRepository;
    private readonly IRepository<ChannelMember> channelMemberRepository;
    private readonly ICurrentUserService currentUser;

    private static readonly char[] alphabet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); // без 0 O 1 I

    public CreateClassroomHandler(
        IRepository<Classroom> classroomRepository,
        IRepository<ClassMember> classMemberRepository,
        IRepository<Channel> channelRepository,
        IRepository<ChannelMember> channelMemberRepository,
        ICurrentUserService currentUser)
    {
        this.classroomRepository = classroomRepository;
        this.classMemberRepository = classMemberRepository;
        this.channelRepository = channelRepository;
        this.channelMemberRepository = channelMemberRepository;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateClassroomCommand request, CancellationToken ct)
    {
        var ownerId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var joinCode = await GenerateUniqueJoinCodeAsync(ct);

        // 1) Створюємо Classroom
        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = request.Title,
            Section = request.Section,
            Description = request.Description,
            JoinCode = joinCode,
            IsArchived = false
        };
        await classroomRepository.AddAsync(classroom);

        // 2) Додаємо власника як учасника класу
        var ownerMember = new ClassMember
        {
            Id = Guid.NewGuid(),
            ClassroomId = classroom.Id,
            UserId = ownerId,
            RoleInClass = ClassRole.Teacher
        };
        await classMemberRepository.AddAsync(ownerMember);

        // 3) Створюємо канал "General" для класу
        var generalChannel = new Channel
        {
            Id = Guid.NewGuid(),
            ClassroomId = classroom.Id,
            Title = "General",
            Type = ChannelType.Classroom // або свій варіант з enum
        };
        await channelRepository.AddAsync(generalChannel);

        // 4) Додаємо вчителя до каналу "General"
        var generalChannelMember = new ChannelMember
        {
            Id = Guid.NewGuid(),
            ChannelId = generalChannel.Id,
            UserId = ownerId,
            IsMuted = false
        };
        await channelMemberRepository.AddAsync(generalChannelMember);

        // 5) Одна транзакція на всі зміни
        await classroomRepository.SaveChangesAsync();

        return classroom.Id;
    }

    private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken ct)
    {
        while (true)
        {
            var code = RandomCode(6);
            var exists = await classroomRepository
                .GetFirstBySpecAsync(new ClassroomByJoinCodeSpec(code)) != null;

            if (!exists) return code;
        }
    }

    private static string RandomCode(int length)
    {
        var rng = Random.Shared;
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = alphabet[rng.Next(alphabet.Length)];
        return new string(chars);
    }
}
