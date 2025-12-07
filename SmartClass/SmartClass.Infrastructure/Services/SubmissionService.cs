// SmartClass.Infrastructure.Services/SubmissionService.cs
using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts;
using SmartClass.Application.Contracts.Submissions;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services
{
    public sealed class SubmissionService : ISubmissionService
    {
        private readonly IRepository<Submission> _submissions;
        private readonly IRepository<Assignment> _assignments;
        private readonly IRepository<FileResource> _files;
        private readonly IRepository<Grade> _grades;
        private readonly IDbContextFactory<AppDbContext> dbFactory;

        public SubmissionService(
            IRepository<Submission> submissions,
            IRepository<Assignment> assignments,
            IRepository<FileResource> files,
            IRepository<Grade> grades,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            _submissions = submissions;
            _assignments = assignments;
            _files = files;
            _grades = grades;
            this.dbFactory = dbFactory;
        }

        public async Task<SubmissionDetailsDto> EnsureForStudentAsync(
            Guid assignmentId,
            Guid studentId,
            CancellationToken ct = default)
        {
            await using var _db = await dbFactory.CreateDbContextAsync(ct);
            var existing = await _db.Submissions
                .Include(s => s.Files)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, ct);

            if (existing is null)
            {
                existing = new Submission
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    Status = SubmissionStatus.NotSubmitted,
                    SubmittedAt = null
                };

                await _submissions.AddAsync(existing);
                await _submissions.SaveChangesAsync();
            }

            var files = existing.Files
                .Select(f => new FileResourceDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    SizeBytes = f.SizeBytes,
                    ContentType = f.ContentType,
                    Url = $"/api/files/{f.Id}/download"
                })
                .ToList();

            return new SubmissionDetailsDto(
                existing.Id,
                existing.AssignmentId,
                existing.StudentId,
                StudentName: "", // можна заповнити через join до AspNetUsers або окремим сервісом
                existing.Status,
                existing.SubmittedAt,
                existing.Grade?.Score,
                existing.Grade?.Comment,
                files
            );
        }

        public async Task<SubmissionDetailsDto?> GetByIdAsync(Guid submissionId, CancellationToken ct = default)
        {

            await using var _db = await dbFactory.CreateDbContextAsync(ct);
            var s = await _db.Submissions
                .Include(x => x.Files)
                .Include(x => x.Grade)
                .FirstOrDefaultAsync(x => x.Id == submissionId, ct);

            if (s is null) return null;

            var files = s.Files
                .Select(f => new FileResourceDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    SizeBytes = f.SizeBytes,
                    ContentType = f.ContentType,
                    Url = $"/api/files/{f.Id}/download"
                })
                .ToList();

            return new SubmissionDetailsDto(
                s.Id,
                s.AssignmentId,
                s.StudentId,
                StudentName: "",
                s.Status,
                s.SubmittedAt,
                s.Grade?.Score,
                s.Grade?.Comment,
                files
            );
        }

        public async Task<IReadOnlyList<SubmissionListItemDto>> GetForAssignmentAsync(
            Guid assignmentId,
            CancellationToken ct = default)
        {
            await using var _db = await dbFactory.CreateDbContextAsync(ct);
            var list = await _db.Submissions
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync(ct);

            return list.Select(s =>
                new SubmissionListItemDto(
                    s.Id,
                    s.StudentId,
                    StudentName: "", // аналогічно — можна підтягти з AspNetUsers
                    s.SubmittedAt,
                    s.Status,
                    s.Grade?.Score
                )).ToList();
        }

        public async Task GradeAsync(GradeSubmissionDto dto, Guid teacherId, CancellationToken ct = default)
        {
            await using var _db = await dbFactory.CreateDbContextAsync(ct);
            var s = await _db.Submissions
                .Include(x => x.Grade)
                .FirstOrDefaultAsync(x => x.Id == dto.SubmissionId, ct);

            if (s is null)
                throw new InvalidOperationException("Submission not found");

            if (s.Grade is null)
            {
                var grade = new Grade
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = s.Id,
                    Score = dto.Score,
                    Comment = dto.Comment,
                    GradedAt = DateTime.UtcNow,
                    GradedBy = teacherId
                };

                await _grades.AddAsync(grade);
                s.Grade = grade;
            }
            else
            {
                s.Grade.Score = dto.Score;
                s.Grade.Comment = dto.Comment;
                s.Grade.GradedAt = DateTime.UtcNow;
                s.Grade.GradedBy = teacherId;
                await _grades.UpdateAsync(s.Grade);
            }

            s.Status = SubmissionStatus.Graded;

            await _submissions.UpdateAsync(s);
            await _submissions.SaveChangesAsync();
        }
    }
}
