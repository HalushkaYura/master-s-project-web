using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Features.Files.Commands.UploadSubmissionFiles;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FilesController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly IFileStorage storage;

    public FilesController(IMediator mediator, IFileStorage storage)
    {
        this.mediator = mediator;
        this.storage = storage;
    }

    /// <summary>Завантаження файлів до submission (Student)</summary>
    [HttpPost("upload/submission")]
    [Authorize(Roles = "Student")]
    [RequestSizeLimit(104857600)] // 100MB (приклад)
    public async Task<IActionResult> UploadForSubmission([FromQuery] Guid submissionId, CancellationToken ct)
    {
        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
            return BadRequest("No files uploaded.");

        var files = new List<UploadFileItem>();
        foreach (var file in Request.Form.Files)
        {
            files.Add(new UploadFileItem
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                Content = file.OpenReadStream()
            });
        }

        var cmd = new UploadSubmissionFilesCommand
        {
            SubmissionId = submissionId,
            Files = files
        };

        var ids = await mediator.Send(cmd, ct);
        return Ok(new { fileIds = ids });
    }

    /// <summary>Скачати файл за FileResource Id</summary>
    [HttpGet("{fileId:guid}/download")]
    [Authorize]
    public async Task<IActionResult> Download([FromRoute] Guid fileId, [FromServices] SmartClass.Application.Abstractions.IRepository<SmartClass.Domain.Entities.FileResource> repo, CancellationToken ct)
    {
        var fr = await repo.GetByKeyAsync(fileId);
        if (fr is null) return NotFound();

        var stream = await storage.OpenReadAsync(fr.BlobPath, ct);
        return File(stream, fr.ContentType, fr.FileName);
    }
}
