using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace SmartClass.Infrastructure.Services
{
    public sealed class ClassroomService : IClassroomService
    {
        private readonly IRepository<Classroom> classrooms;
        private readonly IRepository<ClassMember> classMembers;
        private readonly IRepository<Channel> channels;             
        private readonly IRepository<ChannelMember> channelMembers;
        public ClassroomService(
                   IRepository<Classroom> classrooms,
                   IRepository<ClassMember> classMembers,
                   IRepository<Channel> channels,
                   IRepository<ChannelMember> channelMembers)
        {
            this.classrooms = classrooms;
            this.classMembers = classMembers;
            this.channels = channels;
            this.channelMembers = channelMembers;
        }

        public async Task<ClassroomDto> CreateClassroomAsync(
     CreateClassroomDto dto,
     Guid ownerId,
     CancellationToken ct = default)
        {
            var joinCode = await GenerateUniqueJoinCodeAsync(ct);

            var classroom = new Classroom
            {
                OwnerId = ownerId,
                Title = dto.Title,
                Section = dto.Section,
                Description = dto.Description,
                JoinCode = joinCode,
                IsArchived = false
            };

            await classrooms.AddAsync(classroom);

            // 1) Додаємо викладача як ClassMember
            var classMember = new ClassMember
            {
                ClassroomId = classroom.Id,
                UserId = ownerId,
                RoleInClass = ClassRole.Teacher
            };
            await classMembers.AddAsync(classMember);

            // 2) Створюємо канал для цього класу
            var channel = new Channel
            {
                ClassroomId = classroom.Id,
                Type = ChannelType.Class,
                Title = "Груповий чат учасників класу"
                // Type = ChannelType.Class, якщо є такий enum
            };
            await channels.AddAsync(channel);

            // 3) Додаємо викладача як учасника каналу
            var channelMember = new ChannelMember
            {
                ChannelId = channel.Id,
                UserId = ownerId
                // Можна додати Role = ChannelRole.Owner, якщо є
            };
            await channelMembers.AddAsync(channelMember);

            // 4) Зберігаємо все
            await classrooms.SaveChangesAsync();

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
    Guid ApplicationUserId,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                throw new ArgumentException("Join code is required.", nameof(joinCode));

            var classroom = await classrooms.GetEntityAsync(
                c => c.JoinCode == joinCode && !c.IsArchived);

            if (classroom == null)
                throw new InvalidOperationException("Classroom with this join code was not found or is archived.");

            // 1) Перевіряємо ClassMember
            var existingMember = await classMembers.GetEntityAsync(
                m => m.ClassroomId == classroom.Id && m.UserId == ApplicationUserId);

            if (existingMember == null)
            {
                var member = new ClassMember
                {
                    ClassroomId = classroom.Id,
                    UserId = ApplicationUserId,
                    RoleInClass = ClassRole.Student
                };
                await classMembers.AddAsync(member);
            }

            // 2) Знаходимо канал(и) цього класу (поки робимо один "основний")
            var classChannel = await channels.GetEntityAsync(
                ch => ch.ClassroomId == classroom.Id);

            if (classChannel != null)
            {
                var existingChannelMember = await channelMembers.GetEntityAsync(
                    cm => cm.ChannelId == classChannel.Id && cm.UserId == ApplicationUserId);

                if (existingChannelMember == null)
                {
                    var chMember = new ChannelMember
                    {
                        ChannelId = classChannel.Id,
                        UserId = ApplicationUserId
                    };
                    await channelMembers.AddAsync(chMember);
                }
            }

            await classMembers.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<MyClassroomDto>> GetMyClassroomsAsync(
            Guid ApplicationUserId,
            CancellationToken ct = default)
        {
            // Забираємо всі членства користувача
            var memberships = await classMembers.GetListAsync(
                filter: m => m.UserId == ApplicationUserId,
                includeProperties: "Classroom");

            var list = new List<MyClassroomDto>();

            foreach (var m in memberships)
            {
                // Якщо в GetListAsync includeProperties працює – Classroom буде завантажений
                var classroom = (m as dynamic).Classroom as Classroom;

                if (classroom == null)
                {
                    // fallback: можна було б ще раз підвантажити по ClassroomId
                    classroom = await classrooms.GetByKeyAsync(m.ClassroomId);
                    if (classroom == null) continue;
                }

                list.Add(new MyClassroomDto(
                    classroom.Id,                // Id
                    classroom.Title,             // Title
                    classroom.Section,           // Section
                    m.RoleInClass.ToString(),    // RoleInClass
                    classroom.JoinCode           // JoinCode
                ));
            }

            return list;
        }

        /// <summary>
        /// Проста генерація 6-значного коду й перевірка, що він унікальний.
        /// </summary>
        private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken ct)
        {
            while (true)
            {
                var code = GenerateCode(6);

                var existing = await classrooms.GetEntityAsync(c => c.JoinCode == code);
                if (existing == null)
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


    }
}
