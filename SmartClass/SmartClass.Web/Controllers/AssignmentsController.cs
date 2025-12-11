using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Features.Assignments.Commands;
using SmartClass.Application.Features.Assignments.Queries;
using SmartClass.Infrastructure.Services;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly IAssignmentService assignmentService;

    public AssignmentsController(IMediator mediator, IAssignmentService assignmentService)
    {
        this.mediator = mediator;
        this.assignmentService = assignmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { assignmentId = id });
    }

    [HttpGet("{classroomId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByClassroom(Guid classroomId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAssignmentsByClassroomQuery(classroomId), ct);
        return Ok(result);
    }

    [HttpPost("{assignmentId:guid}/files")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UploadAttachment(Guid assignmentId, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не передано.");

        if (!Guid.TryParse(HttpContext.Request.Query["classroomId"], out var classroomId))
            return BadRequest("classroomId required");

        var ownerId = Guid.Parse(User.FindFirst("sub")!.Value);
        await using var stream = file.OpenReadStream();

        var dto = await assignmentService.UploadAttachmentAsync(
            classroomId,
            assignmentId,
            ownerId,
            file.FileName,
            file.ContentType ?? "application/octet-stream",
            file.Length,
            stream,
            ct);

        return Ok(dto);
    }

}
