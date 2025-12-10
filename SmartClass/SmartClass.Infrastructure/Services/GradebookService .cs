using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Gradebook;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services
{
    public sealed class GradebookService : IGradebookService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IUserService _userService;

        public GradebookService(
            IDbContextFactory<AppDbContext> dbFactory,
            IUserService userService)
        {
            _dbFactory = dbFactory;
            _userService = userService;
        }

        public async Task<GradebookDto> GetForClassroomAsync(
            Guid classroomId,
            CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            // 0) Клас / курс
            var classroom = await db.Classrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == classroomId, ct);

            if (classroom is null)
                throw new InvalidOperationException("Classroom not found");

            // 1) Усі завдання класу
            var assignments = await db.Assignments
                .Where(a => a.ClassroomId == classroomId)
                .OrderBy(a => a.DueAt ?? DateTime.MaxValue)
                .ToListAsync(ct);

            var assignmentIds = assignments.Select(a => a.Id).ToList();

            // 2) Усі учасники класу
            var members = await db.ClassMembers
                .Where(m => m.ClassroomId == classroomId)
                .ToListAsync(ct);

            // Студенти (по ролі в класі)
            var studentMembers = members
                .Where(m =>
                    m.RoleInClass.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                    m.RoleInClass.Contains("студент", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var studentIds = studentMembers
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            // 3) Усі submissions + оцінки по завданням цього класу
            var submissions = await db.Submissions
                .Include(s => s.Grade)
                .Where(s => assignmentIds.Contains(s.AssignmentId))
                .ToListAsync(ct);

            // 4) Імена студентів через UserService
            var studentNames = new Dictionary<Guid, string>();

            foreach (var sid in studentIds)
            {
                var name = await GetStudentNameSafeAsync(sid.ToString());
                studentNames[sid] = name;
            }

            // 5) Заголовки завдань для журналу
            var assignmentHeaders = assignments
                .Select(a => new GradebookAssignmentDto(
                    AssignmentId: a.Id,
                    Title: a.Title,
                    MaxPoints: a.PointsMax,     // поле в твоїй сутності Assignment
                    DueAt: a.DueAt,
                    Status: a.Status.ToString() // якщо Status — enum, можна ToString()
                ))
                .ToList();

            // 6) Побудова рядків по студентах
            var rows = new List<GradebookRowDto>();

            foreach (var student in studentMembers)
            {
                var sid = student.UserId;

                var cells = new List<GradebookCellDto>();

                foreach (var a in assignments)
                {
                    var sub = submissions
                        .FirstOrDefault(s =>
                            s.AssignmentId == a.Id &&
                            s.StudentId == sid);

                    if (sub is null)
                    {
                        // Немає відповіді
                        cells.Add(new GradebookCellDto(
                            AssignmentId: a.Id,
                            SubmissionId: null,
                            Status: SubmissionStatus.NotSubmitted,
                            SubmittedAt: null,
                            Score: null,
                            Comment: null,
                            MaxPoints: a.PointsMax
                        ));
                    }
                    else
                    {
                        cells.Add(new GradebookCellDto(
                            AssignmentId: a.Id,
                            SubmissionId: sub.Id,
                            Status: sub.Status,
                            SubmittedAt: sub.SubmittedAt,
                            Score: sub.Grade?.Score,
                            Comment: sub.Grade?.Comment,
                            MaxPoints: a.PointsMax
                        ));
                    }
                }

                rows.Add(new GradebookRowDto(
                    StudentId: sid,
                    StudentName: studentNames.TryGetValue(sid, out var fullName) ? fullName : string.Empty,
                    Cells: cells
                ));
            }

            return new GradebookDto(
                ClassroomId: classroomId,
                ClassroomTitle: classroom.Title,
                Assignments: assignmentHeaders,
                Rows: rows
            );
        }

        /// <summary>
        /// Безпечне отримання ПІБ студента з Identity через IUserService.
        /// Якщо щось падає – повертаємо пустий рядок, щоб не роняти весь журнал.
        /// </summary>
        private async Task<string> GetStudentNameSafeAsync(string userId)
        {
            try
            {
                var info = await _userService.UserInfoAsync(userId);
                var fullName = $"{info.Lastname} {info.Firstname}".Trim();

                if (!string.IsNullOrWhiteSpace(fullName))
                    return fullName;

                return info.Email ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
