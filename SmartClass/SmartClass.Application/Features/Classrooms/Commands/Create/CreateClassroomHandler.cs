using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Features.Classrooms.Create;
using SmartClass.Application.Features.Classrooms.Specifications;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Classrooms.Commands.Create;

public sealed class CreateClassroomHandler : IRequestHandler<CreateClassroomCommand, Guid>
{
    private readonly IRepository<Classroom> repository;
    private readonly ICurrentUser currentUser;
    private static readonly char[] alphabet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); // без 0 O 1 I

    public CreateClassroomHandler(IRepository<Classroom> repository, ICurrentUser currentUser)
    {
        this.repository = repository;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateClassroomCommand request, CancellationToken ct)
    {
        var ownerId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var joinCode = await GenerateUniqueJoinCodeAsync(ct);

        var entity = new Classroom
        {
            OwnerId = ownerId,
            Title = request.Title,
            Section = request.Section,
            Description = request.Description,
            JoinCode = joinCode,
            IsArchived = false
        };

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();
        return entity.Id;
    }

    private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken ct)
    {
        // 6 символів достатньо; за потреби зроби 7–8
        while (true)
        {
            var code = RandomCode(6);
            var exists = await repository.GetFirstBySpecAsync(new ClassroomByJoinCodeSpec(code)) != null;
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
