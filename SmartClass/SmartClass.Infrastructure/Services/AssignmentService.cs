using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Contracts;
using SmartClass.Application.Contracts.Assignments;
using SmartClass.Application.Contracts.Notifications;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;
using System.Text.Json;

namespace SmartClass.Infrastructure.Services
{
    public sealed class AssignmentService : IAssignmentService
    {
        private readonly IDbContextFactory<AppDbContext> dbFactory;
        private readonly IFileStorage fileStorage;
        private readonly INotificationService _notifications;

        public AssignmentService(
            IDbContextFactory<AppDbContext> dbFactory,
            IFileStorage fileStorage,
            INotificationService notifications)
        {
            this.dbFactory = dbFactory;
            this.fileStorage = fileStorage;
            _notifications = notifications;
        }

        /// <summary>
        /// Допоміжний метод: розіслати нотифікації студентам класу про опубліковане завдання.
        /// Викликається, коли статус завдання вперше стає Published.
        /// </summary>
        private async Task NotifyAssignmentPublishedAsync(
            AppDbContext db,
            Assignment assignment,
            CancellationToken ct)
        {
            // Усі студенти класу
            var studentIds = await db.ClassMembers
                .Where(m => m.ClassroomId == assignment.ClassroomId &&
                            (m.RoleInClass.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                             m.RoleInClass.Contains("студент", StringComparison.OrdinalIgnoreCase)))
                .Select(m => m.UserId)
                .Distinct()
                .ToListAsync(ct);

            if (!studentIds.Any())
                return;

            var payload = JsonSerializer.Serialize(new
            {
                AssignmentId = assignment.Id,
                assignment.Title,
                assignment.DueAt
            });

            foreach (var sid in studentIds)
            {
                await _notifications.CreateAsync(
                    new CreateNotificationDto(
                        UserId: sid,
                        Type: "AssignmentPublished",
                        PayloadJson: payload),
                    ct);
            }
        }

        // 🔹 Створити нове завдання
        public async Task<AssignmentShortDto> CreateAsync(
            CreateAssignmentDto dto,
            Guid teacherId,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var assignment = new Assignment
            {
                ClassroomId = dto.ClassroomId,
                CreatedBy = teacherId,
                MaterialId = dto.MaterialId,
                Title = dto.Title,
                DescriptionHtml = dto.DescriptionHtml,
                PointsMax = dto.PointsMax,
                DueAt = dto.DueAt,
                AllowLate = dto.AllowLate,
                Status = "Draft" // завжди як чорновик
            };

            await db.Assignments.AddAsync(assignment, ct);
            await db.SaveChangesAsync(ct); // тут уже є assignment.Id

            // Тимчасові файли, привʼязані до класу/викладача,
            // але ще без AssignmentId – прикріпляємо до цього завдання
            var tempFiles = await db.Files
                .Where(f =>
                    f.OwnerId == teacherId &&
                    f.ClassroomId == dto.ClassroomId &&
                    f.MaterialId == null &&
                    f.AssignmentId == null)
                .ToListAsync(ct);

            if (tempFiles.Any())
            {
                foreach (var f in tempFiles)
                {
                    f.AssignmentId = assignment.Id;
                }

                await db.SaveChangesAsync(ct);
            }

            return new AssignmentShortDto(
                assignment.Id,
                assignment.ClassroomId,
                assignment.CreatedBy,
                assignment.Title,
                assignment.DescriptionHtml,
                assignment.PointsMax,
                assignment.DueAt,
                assignment.AllowLate,
                assignment.Status
            );
        }

        // 🔹 Отримати всі завдання для класу
        public async Task<IReadOnlyList<AssignmentShortDto>> GetForClassroomAsync(
            Guid classroomId,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var list = await db.Assignments
                .Where(a => a.ClassroomId == classroomId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);

            return list
                .Select(a => new AssignmentShortDto(
                    a.Id,
                    a.ClassroomId,
                    a.CreatedBy,
                    a.Title,
                    a.DescriptionHtml,
                    a.PointsMax,
                    a.DueAt,
                    a.AllowLate,
                    a.Status
                ))
                .ToList();
        }

        // 🔹 Оновити завдання (і, якщо потрібно, розіслати нотифікації)
        public async Task UpdateAsync(
            UpdateAssignmentDto dto,
            Guid teacherId,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var assignment = await db.Assignments
                .FirstOrDefaultAsync(a => a.Id == dto.Id, ct);

            if (assignment == null)
                throw new InvalidOperationException("Assignment not found.");

            // (опційно) перевірити, що саме цей teacherId є власником
            // if (assignment.CreatedBy != teacherId) throw new UnauthorizedAccessException();

            var wasPublished = string.Equals(assignment.Status, "Published", StringComparison.OrdinalIgnoreCase);

            assignment.Title = dto.Title;
            assignment.DescriptionHtml = dto.DescriptionHtml;
            assignment.PointsMax = dto.PointsMax;
            assignment.DueAt = dto.DueAt;
            assignment.AllowLate = dto.AllowLate;
            assignment.Status = dto.Status;

            await db.SaveChangesAsync(ct);

            var isPublishedNow = string.Equals(assignment.Status, "Published", StringComparison.OrdinalIgnoreCase);

            // Якщо завдання стало Published вперше – надсилаємо нотифікації студентам
            if (!wasPublished && isPublishedNow)
            {
                await NotifyAssignmentPublishedAsync(db, assignment, ct);
            }
        }

        public async Task DeleteAsync(Guid assignmentId, Guid teacherId, CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var assignment = await db.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId, ct);

            if (assignment == null)
                return;

            // (опційно) перевірка власника
            // if (assignment.CreatedBy != teacherId) throw new UnauthorizedAccessException();

            db.Assignments.Remove(assignment);
            await db.SaveChangesAsync(ct);
        }

        // 🔹 Завантажити вкладення для завдання
        public async Task<FileResourceDto> UploadAttachmentAsync(
            Guid classroomId,
            Guid assignmentId,
            Guid ownerId,
            string fileName,
            string contentType,
            long sizeBytes,
            Stream content,
            CancellationToken ct = default)
        {
            var relativePath = $"assignments/{classroomId}/{assignmentId}/{Guid.NewGuid()}_{fileName}";
            var blobPath = await fileStorage.SaveAsync(content, relativePath, ct);

            var file = new FileResource
            {
                OwnerId = ownerId,
                ClassroomId = classroomId,
                AssignmentId = assignmentId,
                FileName = fileName,
                ContentType = contentType,
                SizeBytes = sizeBytes,
                BlobPath = blobPath,
                UploadedAt = DateTime.UtcNow
            };

            await using var db = await dbFactory.CreateDbContextAsync(ct);

            await db.Files.AddAsync(file, ct);
            await db.SaveChangesAsync(ct);

            return new FileResourceDto
            {
                Id = file.Id,
                FileName = file.FileName,
                SizeBytes = file.SizeBytes,
                ContentType = file.ContentType,
                Url = $"/api/files/{file.Id}/download"
            };
        }

        // 🔹 Отримати деталі завдання
        public async Task<AssignmentDetailsDto?> GetByIdAsync(
            Guid classroomId,
            Guid assignmentId,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var a = await db.Assignments
                .Include(x => x.Files)
                .Include(x => x.Material)
                .FirstOrDefaultAsync(x =>
                    x.Id == assignmentId &&
                    x.ClassroomId == classroomId,
                    ct);

            if (a is null) return null;

            var files = a.Files
                .OrderBy(f => f.FileName)
                .Select(f => new FileResourceDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    SizeBytes = f.SizeBytes,
                    ContentType = f.ContentType,
                    Url = $"/api/files/{f.Id}/download"
                })
                .ToList();

            return new AssignmentDetailsDto(
                a.Id,
                a.ClassroomId,
                a.CreatedBy,
                a.Title,
                a.DescriptionHtml,
                a.PointsMax,
                a.DueAt,
                a.AllowLate,
                a.Status,
                a.MaterialId,
                a.Material?.Title,
                files
            );
        }
    }
}
