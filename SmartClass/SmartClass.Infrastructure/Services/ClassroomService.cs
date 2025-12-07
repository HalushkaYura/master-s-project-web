using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace SmartClass.Infrastructure.Services
{
    public sealed class ClassroomService : IClassroomService
    {
        private readonly IDbContextFactory<AppDbContext> dbFactory;

        public ClassroomService(IDbContextFactory<AppDbContext> dbContext)
        {
            this.dbFactory = dbContext;
        }

        public async Task<ClassroomDto> CreateClassroomAsync(
            CreateClassroomDto dto,
            Guid ownerId,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);
            // 🔹 (опційно) перевіряємо, що користувач взагалі існує
            var ownerExists = await db.Users.AnyAsync(u => u.Id == ownerId, ct);
            if (!ownerExists)
                throw new InvalidOperationException("Owner user does not exist.");

            // 1) генеруємо код
            var joinCode = await GenerateUniqueJoinCodeAsync(ct);

            // 2) Classroom
            var classroom = new Classroom
            {
                Id = Guid.NewGuid(),   // 👈 явно задаємо Id (якщо BaseEntity не робить цього)
                OwnerId = ownerId,
                Title = dto.Title,
                Section = dto.Section,
                Description = dto.Description,
                JoinCode = joinCode,
                IsArchived = false
            };

            // 3) викладач як ClassMember
            var classMember = new ClassMember
            {
                Id = Guid.NewGuid(),
                ClassroomId = classroom.Id,     // посилання на щойно згенерований Id
                UserId = ownerId,
                RoleInClass = ClassRole.Teacher.ToString()
            };

            // 4) канал для класу
            var channel = new Channel
            {
                Id = Guid.NewGuid(),
                ClassroomId = classroom.Id,
                Type = ChannelType.Class,
                Title = "Груповий чат учасників класу"
            };

            // 5) викладач як учасник каналу
            var channelMember = new ChannelMember
            {
                Id = Guid.NewGuid(),
                ChannelId = channel.Id,
                UserId = ownerId,
                IsMuted = false
            };

            // 6) додаємо ВСЕ в один DbContext
            await db.Classrooms.AddAsync(classroom, ct);
            await db.ClassMembers.AddAsync(classMember, ct);
            await db.Channels.AddAsync(channel, ct);
            await db.ChannelMembers.AddAsync(channelMember, ct);

            // 7) один SaveChangesAsync → одна транзакція
            await db.SaveChangesAsync(ct);

            return new ClassroomDto
            {
                Id = classroom.Id,
                Title = classroom.Title,
                Section = classroom.Section,
                JoinCode = classroom.JoinCode,
                Description = classroom.Description,
                IsArchived = classroom.IsArchived,
                RoleInClass = "Teacher"
            };
        }

        public async Task JoinClassroomAsync(
            string joinCode,
            Guid userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                throw new ArgumentException("Join code is required.", nameof(joinCode));
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var classroom = await db.Classrooms
                .FirstOrDefaultAsync(c => c.JoinCode == joinCode && !c.IsArchived, ct);

            if (classroom == null)
                throw new InvalidOperationException("Classroom with this join code was not found or is archived.");

            // (опційно) перевірка існування користувача
            var userExists = await db.Users.AnyAsync(u => u.Id == userId, ct);
            if (!userExists)
                throw new InvalidOperationException("User does not exist.");

            // 1) ClassMember
            var existingMember = await db.ClassMembers
                .FirstOrDefaultAsync(m => m.ClassroomId == classroom.Id && m.UserId == userId, ct);

            if (existingMember == null)
            {
                var member = new ClassMember
                {
                    Id = Guid.NewGuid(),
                    ClassroomId = classroom.Id,
                    UserId = userId,
                    RoleInClass = ClassRole.Student.ToString()
                };
                await db.ClassMembers.AddAsync(member, ct);
            }

            // 2) канал цього класу
            var classChannel = await db.Channels
                .FirstOrDefaultAsync(ch => ch.ClassroomId == classroom.Id, ct);

            if (classChannel != null)
            {
                var existingChannelMember = await db.ChannelMembers
                    .FirstOrDefaultAsync(cm => cm.ChannelId == classChannel.Id && cm.UserId == userId, ct);

                if (existingChannelMember == null)
                {
                    var chMember = new ChannelMember
                    {
                        Id = Guid.NewGuid(),
                        ChannelId = classChannel.Id,
                        UserId = userId,
                        IsMuted = false
                    };
                    await db.ChannelMembers.AddAsync(chMember, ct);
                }
            }

            await db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<MyClassroomDto>> GetMyClassroomsAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var memberships = await db.ClassMembers
                .Include(m => m.Classroom)
                .Where(m => m.UserId == userId)
                .ToListAsync(ct);

            var list = memberships
                .Where(m => m.Classroom != null)
                .Select(m => new MyClassroomDto(
                    m.Classroom!.Id,
                    m.Classroom.Title,
                    m.Classroom.Section,
                    m.RoleInClass.ToString(),
                    m.Classroom.JoinCode
                ))
                .ToList();

            return list;
        }

        private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken ct)
        {
            while (true)
            {
                var code = GenerateCode(6);
                await using var db = await dbFactory.CreateDbContextAsync(ct);

                var exists = await db.Classrooms.AnyAsync(c => c.JoinCode == code, ct);
                if (!exists)
                    return code;
            }
        }

        private static string GenerateCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var sb = new StringBuilder(length);
            foreach (var b in bytes)
            {
                sb.Append(chars[b % chars.Length]);
            }
            return sb.ToString();
        }

        public async Task<ClassroomDetailsDto> GetClassroomDetailsAsync(
    Guid classroomId,
    Guid currentUserId,
    CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var classroom = await db.Classrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classroomId, ct);

            if (classroom == null)
                throw new KeyNotFoundException("Classroom not found.");

            // (опціонально) перевірка, що користувач є членом класу
            var isMember = await db.ClassMembers
                .AnyAsync(m => m.ClassroomId == classroomId && m.UserId == currentUserId, ct);

            if (!isMember && classroom.OwnerId != currentUserId)
                throw new InvalidOperationException("You are not a member of this classroom.");

            // беремо усіх учасників з ApplicationUser
            var members = await db.ClassMembers
                .Where(m => m.ClassroomId == classroomId)
                .Join(
                    db.Users,
                    m => m.UserId,
                    u => u.Id,
                    (m, u) => new ClassroomMemberDto(
                        u.Id,
                        (u.DisplayName ??
                         $"{(u.Firstname ?? "").Trim()} {(u.Lastname ?? "").Trim()}".Trim()),
                        u.Email ?? string.Empty,
                        m.RoleInClass.ToString()
                    )
                )
                .ToListAsync(ct);

            var ownerName = members
                .FirstOrDefault(x => x.UserId == classroom.OwnerId)?.FullName
                ?? "Teacher";

            return new ClassroomDetailsDto(
                classroom.Id,
                classroom.Title,
                classroom.Section,
                classroom.Description,
                classroom.JoinCode,
                classroom.IsArchived,
                ownerName,
                members
            );
        }

        public async Task<ClassroomDetailsDto> GetClassroomByJoinCodeAsync(
    string joinCode,
    CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var classroom = await db.Classrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.JoinCode == joinCode && !x.IsArchived, ct);

            if (classroom == null)
                throw new InvalidOperationException("Classroom not found or archived.");

            var members = await db.ClassMembers
                .Where(m => m.ClassroomId == classroom.Id)
                .Join(
                    db.Users,
                    m => m.UserId,
                    u => u.Id,
                    (m, u) => new ClassroomMemberDto(
                        u.Id,
                        u.DisplayName ??
                        $"{u.Firstname} {u.Lastname}",
                        u.Email ?? "",
                        m.RoleInClass.ToString()
                    )
                )
                .ToListAsync(ct);

            var ownerName = members
                .FirstOrDefault(x => x.UserId == classroom.OwnerId)?.FullName
                ?? "Teacher";

            return new ClassroomDetailsDto(
                classroom.Id,
                classroom.Title,
                classroom.Section,
                classroom.Description,
                classroom.JoinCode,
                classroom.IsArchived,
                ownerName,
                members
            );
        }

    }
}
