using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Assignments;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services;

public sealed class AssignmentService : IAssignmentService
{
    private readonly AppDbContext db;

    public AssignmentService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<AssignmentDto> CreateAsync(
        CreateAssignmentDto dto,
        Guid teacherId,
        CancellationToken ct = default)
    {
        var assignment = new Assignment
        {
            ClassroomId = dto.ClassroomId,
            CreatedBy = teacherId,
            Title = dto.Title,
            DescriptionHtml = dto.DescriptionHtml,
            PointsMax = dto.PointsMax,
            DueAt = dto.DueAt,
            AllowLate = dto.AllowLate,
            Status = "Draft"
        };

        await db.Assignments.AddAsync(assignment, ct);
        await db.SaveChangesAsync(ct);

        return new AssignmentDto(
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

    public async Task<IReadOnlyList<AssignmentDto>> GetForClassroomAsync(
        Guid classroomId,
        CancellationToken ct = default)
    {
        var list = await db.Assignments
            .Where(a => a.ClassroomId == classroomId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return list
            .Select(a => new AssignmentDto(
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

    public async Task UpdateAsync(
        UpdateAssignmentDto dto,
        Guid teacherId,
        CancellationToken ct = default)
    {
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
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId, ct);
        if (assignment == null) return;

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
    }
}
