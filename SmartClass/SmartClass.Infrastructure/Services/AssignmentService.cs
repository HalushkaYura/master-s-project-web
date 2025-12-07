using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Contracts;
using SmartClass.Application.Contracts.Assignments;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services;

public sealed class AssignmentService : IAssignmentService
{
    private readonly IDbContextFactory<AppDbContext> dbFactory;
    private readonly IFileStorage fileStorage;

    public AssignmentService(IDbContextFactory<AppDbContext> dbFactory, IFileStorage fileStorage)
    {
        this.dbFactory = dbFactory;
        this.fileStorage = fileStorage;
    }
    // 🔹 ОНОВЛЕНИЙ МЕТОД: Створити нове завдання
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
            MaterialId = dto.MaterialId,          // якщо є прив’язка до теми
            Title = dto.Title,
            DescriptionHtml = dto.DescriptionHtml,
            PointsMax = dto.PointsMax,
            DueAt = dto.DueAt,
            AllowLate = dto.AllowLate,
            Status = "Draft"
        };

        await db.Assignments.AddAsync(assignment, ct);
        await db.SaveChangesAsync(ct); // тут уже є assignment.Id

        // 🔹 Після створення завдання — прив'язуємо тимчасові файли в цьому класі для цього вчителя
        var tempFiles = await db.Files
            .Where(f =>
                f.OwnerId == teacherId &&
                f.ClassroomId == dto.ClassroomId &&
                f.MaterialId == null &&           // не прив’язані до матеріалу
                f.AssignmentId == null)          // не прив’язані до завдання
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

    // 🔹 ОНОВЛЕНИЙ МЕТОД: Отримати всі завдання для класу
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
    // 🔹 ОНОВЛЕНИЙ МЕТОД: Оновити завдання
    public async Task UpdateAsync(
        UpdateAssignmentDto dto,
        Guid teacherId,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == dto.Id, ct);
        if (assignment == null)
            throw new InvalidOperationException("Assignment not found.");

        assignment.Title = dto.Title;
        assignment.DescriptionHtml = dto.DescriptionHtml;
        assignment.PointsMax = dto.PointsMax;
        assignment.DueAt = dto.DueAt;
        assignment.AllowLate = dto.AllowLate;
        assignment.Status = dto.Status;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid assignmentId, Guid teacherId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId, ct);
        if (assignment == null) return;

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
    }
    // 🔹 НОВИЙ МЕТОД: Завантажити вкладення для завдання
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
            Url = $"/api/files/{file.Id}"
        };
    }

    // 🔹 НОВИЙ МЕТОД: Отримати завдання за Id з деталями
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
                a.Id,                  // Id
                a.ClassroomId,         // ClassroomId
                a.CreatedBy,           // CreatedBy
                a.Title,               // Title
                a.DescriptionHtml,     // DescriptionHtml
                a.PointsMax,           // PointsMax
                a.DueAt,               // DueAt
                a.AllowLate,           // AllowLate
                a.Status,              // Status
                a.MaterialId,          // MaterialId
                a.Material?.Title,     // MaterialTitle
                files                  // Files
            );
    }

}

