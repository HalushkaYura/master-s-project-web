using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Application.Contracts.Materials;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services;

public sealed class MaterialService : IMaterialService
{
    private readonly AppDbContext db;

    public MaterialService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<MaterialDto> CreateAsync(
        CreateMaterialDto dto,
        Guid teacherId,
        CancellationToken ct = default)
    {
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
        await db.SaveChangesAsync(ct);

        return new MaterialDto(
            material.Id,
            material.ClassroomId,
            material.CreatedBy,
            material.Title,
            material.Description,
            material.ContentHtml,
            material.CreatedAt,
            material.UpdatedAt
        );
    }

    public async Task<IReadOnlyList<MaterialDto>> GetForClassroomAsync(
        Guid classroomId,
        CancellationToken ct = default)
    {
        var list = await db.Materials
            .Where(m => m.ClassroomId == classroomId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return list
            .Select(m => new MaterialDto(
                m.Id,
                m.ClassroomId,
                m.CreatedBy,
                m.Title,
                m.Description,
                m.ContentHtml,
                m.CreatedAt,
                m.UpdatedAt
            ))
            .ToList();
    }

    public async Task UpdateAsync(
        UpdateMaterialDto dto,
        Guid teacherId,
        CancellationToken ct = default)
    {
        var material = await db.Materials.FirstOrDefaultAsync(m => m.Id == dto.Id, ct);
        if (material == null)
            throw new InvalidOperationException("Material not found.");

        // за бажанням можна перевіряти, що teacherId == material.CreatedBy

        material.Title = dto.Title;
        material.Description = dto.Description;
        material.ContentHtml = dto.ContentHtml;
        material.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid materialId, Guid teacherId, CancellationToken ct = default)
    {
        var material = await db.Materials.FirstOrDefaultAsync(m => m.Id == materialId, ct);
        if (material == null) return;

        db.Materials.Remove(material);
        await db.SaveChangesAsync(ct);
    }
}
