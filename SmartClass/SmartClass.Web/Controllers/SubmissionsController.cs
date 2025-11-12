using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Features.Submissions.Commands.Resubmit;
using SmartClass.Application.Features.Submissions.Commands.SubmitAssignment;
using SmartClass.Application.Features.Submissions.Queries.GetByAssignment;
using SmartClass.Application.Features.Submissions.Queries.GetMine;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SubmissionsController : ControllerBase
{
    private readonly IMediator mediator;

    public SubmissionsController(IMediator mediator) => this.mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit([FromBody] SubmitAssignmentCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { submissionId = id });
    }

    [HttpGet("by-assignment/{assignmentId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ByAssignment(Guid assignmentId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSubmissionsByAssignmentQuery(assignmentId), ct);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySubmissionsQuery(), ct);
        return Ok(result);
    }

    [HttpPost("{assignmentId:guid}/resubmit")]
    [Authorize(Roles = "Student")]
    [RequestSizeLimit(104857600)]
    public async Task<IActionResult> Resubmit([FromRoute] Guid assignmentId, CancellationToken ct)
    {
        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
            return BadRequest("No files uploaded.");

        var files = new List<ResubmitFileItem>();
        foreach (var f in Request.Form.Files)
        {
            files.Add(new ResubmitFileItem
            {
                FileName = f.FileName,
                ContentType = f.ContentType,
                SizeBytes = f.Length,
                Content = f.OpenReadStream()
            });
        }

        var cmd = new ResubmitSubmissionCommand
        {
            AssignmentId = assignmentId,
            Files = files
        };

        var id = await mediator.Send(cmd, ct);
        return Ok(new { submissionId = id });
    }
}
