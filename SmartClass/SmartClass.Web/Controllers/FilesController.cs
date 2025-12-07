using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Domain.Entities;
using System.Security.Claims;

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


        /// <summary>Завантаження файлів до МАТЕРІАЛУ</summary>
        [HttpPost("upload/material/{materialId:guid}")]
        [Authorize(Roles = "Teacher")]
        [RequestSizeLimit(104_857_600)]
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

                var safeFileName = Path.GetFileName(file.FileName);
                var relativePath = Path.Combine(
                    "classrooms",
                    classroomId.ToString(),
                    "materials",
                    materialId.ToString(),
                    safeFileName);

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = ownerId,          // тут буде c80627f2-...
                    ClassroomId = classroomId,
                    MaterialId = materialId,
                    FileName = safeFileName,
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

                var safeFileName = Path.GetFileName(file.FileName);
                var relativePath = Path.Combine(
                    "classrooms",
                    classroomId.ToString(),
                    "assignments",
                    assignmentId.ToString(),
                    safeFileName);

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = ownerId,
                    ClassroomId = classroomId,
                    AssignmentId = assignmentId,
                    FileName = safeFileName,
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

                var safeFileName = Path.GetFileName(file.FileName);
                var relativePath = Path.Combine(
                    "classrooms",
                    classroomId.ToString(),
                    "temp",
                    $"{Guid.NewGuid()}_{safeFileName}");

                var blobPath = await storage.SaveAsync(stream, relativePath, ct);

                var fr = new FileResource
                {
                    OwnerId = ownerId,
                    ClassroomId = classroomId,
                    MaterialId = null,
                    AssignmentId = null,
                    SubmissionId = null,
                    FileName = safeFileName,
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



        /// <summary>Скачати файл за FileResource Id</summary>
        [HttpGet("{fileId:guid}/download")]
        [Authorize(Roles = "Teacher,Student")] 
        public async Task<IActionResult> Download(
            [FromRoute] Guid fileId,
            [FromServices] IRepository<FileResource> repo,
            CancellationToken ct)
        {
            var fr = await repo.GetByKeyAsync(fileId);
            if (fr is null) return NotFound();

            var stream = await storage.OpenReadAsync(fr.BlobPath, ct);
            return File(stream, fr.ContentType, fr.FileName);
        }

    }
}
