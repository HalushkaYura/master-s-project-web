using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Contracts;
using SmartClass.Application.Contracts.Assignments;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Application.Contracts.Materials;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

public class MaterialService : IMaterialService
{
    private readonly IDbContextFactory<AppDbContext> dbFactory;
    private readonly IFileStorage fileStorage;

    public MaterialService(IDbContextFactory<AppDbContext> dbFactory, IFileStorage fileStorage)
    {
        this.dbFactory = dbFactory;
        this.fileStorage = fileStorage;
    }

    public async Task<MaterialDto> CreateAsync(CreateMaterialDto dto, Guid teacherId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var material = new Material
        {
            ClassroomId = dto.ClassroomId,
            CreatedBy = teacherId,
            Title = dto.Title,
            Description = dto.Description,
            ContentHtml = dto.ContentHtml,
            UpdatedAt = DateTime.UtcNow
        };

        await db.Materials.AddAsync(material, ct);
        await db.SaveChangesAsync(ct); // тут вже є material.Id

        // 🔹 Після створення матеріалу — прив'язуємо тимчасові файли в цьому класі для цього вчителя
        var tempFiles = await db.Files
            .Where(f =>
                f.OwnerId == teacherId &&
                f.ClassroomId == dto.ClassroomId &&
                f.MaterialId == null &&           // ще не прив’язані до матеріалу
                f.AssignmentId == null)          // і не прив’язані до завдання
            .ToListAsync(ct);

        if (tempFiles.Any())
        {
            foreach (var f in tempFiles)
            {
                f.MaterialId = material.Id;
            }

            await db.SaveChangesAsync(ct);
        }

        return new MaterialDto(
            material.Id,
            material.ClassroomId,
            material.CreatedBy,
            material.Title,
            material.Description,
            material.ContentHtml,
            material.CreatedAt,
            material.UpdatedAt,
            new List<FileResourceDto>(),
            new List<AssignmentShortDto>()
        );
    }


    public async Task<IReadOnlyList<MaterialDto>> GetForClassroomAsync(Guid classroomId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var materials = await db.Materials
            .Include(m => m.Assignments)
            .Include(m => m.Files)
            .Where(m => m.ClassroomId == classroomId)
            .OrderBy(m => m.Title)
            .ToListAsync(ct);

        var result = materials.Select(m => new MaterialDto(
            m.Id,
            m.ClassroomId,
            m.CreatedBy,
            m.Title,
            m.Description,
            m.ContentHtml,
            m.CreatedAt,
            m.UpdatedAt,
            m.Files.Select(f => new FileResourceDto
            {
                Id = f.Id,
                FileName = f.FileName,
                SizeBytes = f.SizeBytes,
                ContentType = f.ContentType,
                Url = $"/api/files/{f.Id}"
            }).ToList(),
            m.Assignments.Select(a => new AssignmentShortDto(
                a.Id,
                a.ClassroomId,
                a.CreatedBy,
                a.Title,
                a.DescriptionHtml,
                a.PointsMax,
                a.DueAt,
                a.AllowLate,
                a.Status
            )).ToList()
        )).ToList();

        return result;
    }

    public async Task UpdateAsync(UpdateMaterialDto dto, Guid teacherId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var material = await db.Materials.FirstOrDefaultAsync(m => m.Id == dto.Id, ct);
        if (material == null)
            throw new InvalidOperationException("Матеріал не знайдено.");

        // (опціонально перевірити, що teacherId має права)

        material.Title = dto.Title;
        material.Description = dto.Description;
        material.ContentHtml = dto.ContentHtml;
        material.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid materialId, Guid teacherId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var material = await db.Materials.FirstOrDefaultAsync(m => m.Id == materialId, ct);
        if (material == null)
            return;

        db.Materials.Remove(material);
        await db.SaveChangesAsync(ct);
    }

    public async Task<FileResourceDto> UploadFileAsync(
        Guid classroomId,
        Guid materialId,
        Guid ownerId,
        string fileName,
        string contentType,
        long sizeBytes,
        Stream content,
        CancellationToken ct = default)
    {
        // 1) Зберігаємо файл у сховище (LocalFileStorage / Azure / інше)
        var relativePath = $"materials/{classroomId}/{materialId}/{Guid.NewGuid()}_{fileName}";
        var blobPath = await fileStorage.SaveAsync(content, relativePath, ct);

        // 2) Створюємо запис у БД
        var file = new FileResource
        {
            OwnerId = ownerId,
            ClassroomId = classroomId,
            MaterialId = materialId,
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

    public async Task<MaterialDto?> GetByIdAsync(Guid classroomId, Guid materialId, CancellationToken ct = default)
    {
        await using var _dbContext = await dbFactory.CreateDbContextAsync(ct);

        var m = await _dbContext.Materials
            .Include(x => x.Files)
            .Include(x => x.Assignments)
            .FirstOrDefaultAsync(x =>
                x.Id == materialId &&
                x.ClassroomId == classroomId, ct);

        if (m is null) return null;

        var files = m.Files
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

        var assignments = m.Assignments
            .OrderBy(a => a.DueAt ?? DateTime.MaxValue)
            .Select(a => new AssignmentShortDto(
                 a.Id,
                 a.ClassroomId,
                 a.CreatedBy,
                 a.Title,
                 a.DescriptionHtml,
                 a.PointsMax,
                 a.DueAt,
                 a.AllowLate,
                 a.Status))
            .ToList();

        return new MaterialDto(
            m.Id,
            m.ClassroomId,
            m.CreatedBy,
            m.Title,
            m.Description,
            m.ContentHtml,
            m.CreatedAt,
            m.UpdatedAt,
            files,
            assignments);
    }


}
