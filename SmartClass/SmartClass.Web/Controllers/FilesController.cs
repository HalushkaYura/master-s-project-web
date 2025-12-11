using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Domain.Entities;
using System.Security.Claims;
using System.IO;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FilesController : ControllerBase
    {
        private readonly IFileStorage storage;

        public FilesController(IFileStorage storage)
        {
            this.storage = storage;
        }

        private bool TryGetUserId(out Guid userId)
        {
            var c =
                User.FindFirst("sub") ??
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (c != null && Guid.TryParse(c.Value, out userId))
                return true;

            userId = Guid.Empty;
            return false;
        }

        private static string BuildStorageName(string originalName)
        {
            var ext = Path.GetExtension(originalName);
            return $"{Guid.NewGuid():N}{ext}";
        }

        /// <summary>Завантаження файлів до МАТЕРІАЛУ</summary>
        [HttpPost("upload/material/{materialId:guid}")]
        [Authorize(Roles = "Teacher")]
        [RequestSizeLimit(104_857_600)] // 100 MB
        public async Task<IActionResult> UploadForMaterial(
            [FromRoute] Guid materialId,
            [FromQuery] Guid classroomId,
            [FromServices] IRepository<FileResource> repo,
            CancellationToken ct)
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                return BadRequest("No files uploaded.");

            if (!TryGetUserId(out var ownerId))
                return Unauthorized("Користувача не знайдено.");

            var savedIds = new List<Guid>();

            foreach (var file in Request.Form.Files)
            {
                using var stream = file.OpenReadStream();

                var originalName = Path.GetFileName(file.FileName);
                var storageName = BuildStorageName(originalName);

                // ⚡ Короткий відносний шлях (без "uploads")
                var relativePath = Path.Combine(
                    "materials",
                    materialId.ToString("N"),
                    storageName);

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = ownerId,
                    ClassroomId = classroomId,
                    MaterialId = materialId,
                    FileName = originalName, // те, що бачить користувач
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    BlobPath = blobPath,     // короткий відносний шлях
                    UploadedAt = DateTime.UtcNow
                };

                await repo.AddAsync(fr);
                savedIds.Add(fr.Id);
            }

            await repo.SaveChangesAsync();
            return Ok(new { fileIds = savedIds });
        }

        /// <summary>Завантаження файлів до ЗАВДАННЯ</summary>
        [HttpPost("upload/assignment/{assignmentId:guid}")]
        [Authorize(Roles = "Teacher")]
        [RequestSizeLimit(104_857_600)] // 100 MB
        public async Task<IActionResult> UploadForAssignment(
            [FromRoute] Guid assignmentId,
            [FromQuery] Guid classroomId,
            [FromServices] IRepository<FileResource> repo,
            CancellationToken ct)
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                return BadRequest("No files uploaded.");

            if (!TryGetUserId(out var ownerId))
                return Forbid();

            var savedIds = new List<Guid>();

            foreach (var file in Request.Form.Files)
            {
                using var stream = file.OpenReadStream();

                var originalName = Path.GetFileName(file.FileName);
                var storageName = BuildStorageName(originalName);

                var relativePath = Path.Combine(
                    "assignments",
                    assignmentId.ToString("N"),
                    storageName);

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = ownerId,
                    ClassroomId = classroomId,
                    AssignmentId = assignmentId,
                    FileName = originalName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    BlobPath = blobPath,
                    UploadedAt = DateTime.UtcNow
                };

                await repo.AddAsync(fr);
                savedIds.Add(fr.Id);
            }

            await repo.SaveChangesAsync();
            return Ok(new { fileIds = savedIds });
        }

        /// <summary>
        /// Тимчасове завантаження файлів для класу під час створення матеріалу/завдання.
        /// Файли будуть без MaterialId/AssignmentId, але зафіксовані за Classroom + Owner.
        /// </summary>
        [HttpPost("upload/temp/classroom/{classroomId:guid}")]
        [Authorize(Roles = "Teacher")]
        [RequestSizeLimit(104_857_600)] // 100 MB
        public async Task<IActionResult> UploadTempForClassroom(
            [FromRoute] Guid classroomId,
            [FromServices] IRepository<FileResource> repo,
            CancellationToken ct)
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                return BadRequest("No files uploaded.");

            if (!TryGetUserId(out var ownerId))
                return Forbid();

            var savedIds = new List<Guid>();

            foreach (var file in Request.Form.Files)
            {
                using var stream = file.OpenReadStream();

                var originalName = Path.GetFileName(file.FileName);
                var storageName = BuildStorageName(originalName);

                var relativePath = Path.Combine(
                    "temp",
                    classroomId.ToString("N"),
                    storageName);

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = ownerId,
                    ClassroomId = classroomId,
                    MaterialId = null,
                    AssignmentId = null,
                    SubmissionId = null,
                    FileName = originalName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    BlobPath = blobPath,
                    UploadedAt = DateTime.UtcNow
                };

                await repo.AddAsync(fr);
                savedIds.Add(fr.Id);
            }

            await repo.SaveChangesAsync();
            return Ok(new { fileIds = savedIds });
        }

        /// <summary>Завантаження файлів до ВІДПОВІДІ (Submission) студента</summary>
        [HttpPost("upload/submission")]
        [Authorize(Roles = "Student")]
        [RequestSizeLimit(104_857_600)] // 100 MB
        public async Task<IActionResult> UploadForSubmission(
            [FromQuery] Guid submissionId,
            [FromServices] IRepository<FileResource> filesRepo,
            [FromServices] IRepository<Submission> submissionsRepo,
            CancellationToken ct)
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                return BadRequest("No files uploaded.");

            if (!TryGetUserId(out var userId))
                return Forbid();

            // 1. Перевіряємо, що сабміт існує і належить цьому студенту
            var submission = await submissionsRepo.GetEntityAsync(
                s => s.Id == submissionId,
                includeProperties: "Assignment");

            if (submission is null)
                return NotFound("Submission not found.");

            if (submission.StudentId != userId)
                return Forbid(); // чужий сабміт — забороняємо

            var savedIds = new List<Guid>();

            foreach (var file in Request.Form.Files)
            {
                using var stream = file.OpenReadStream();

                var originalName = Path.GetFileName(file.FileName);
                var storageName = BuildStorageName(originalName);

                var relativePath = Path.Combine(
                    "submissions",
                    submissionId.ToString("N"),
                    storageName);

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = userId,
                    ClassroomId = submission.Assignment.ClassroomId,
                    SubmissionId = submissionId,
                    FileName = originalName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    BlobPath = blobPath,
                    UploadedAt = DateTime.UtcNow
                };

                await filesRepo.AddAsync(fr);
                savedIds.Add(fr.Id);
            }

            await filesRepo.SaveChangesAsync();

            return Ok(new { fileIds = savedIds });
        }

        /// <summary>Скачати файл за FileResource Id</summary>
        [HttpGet("{fileId:guid}/download")]
        [Authorize]
        public async Task<IActionResult> Download(
            [FromRoute] Guid fileId,
            [FromServices] IRepository<FileResource> filesRepo,
            [FromServices] IFileStorage storage,
            CancellationToken ct)
        {
            var file = await filesRepo.GetByKeyAsync(fileId);
            if (file is null)
                return NotFound();

            var stream = await storage.OpenReadAsync(file.BlobPath, ct);

            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

            var downloadName = string.IsNullOrWhiteSpace(file.FileName)
                ? "file"
                : file.FileName;

            return File(stream, contentType, downloadName);
        }
    }
}
