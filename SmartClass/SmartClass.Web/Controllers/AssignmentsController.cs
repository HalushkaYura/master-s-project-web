using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Features.Assignments.Commands;
using SmartClass.Application.Features.Assignments.Queries;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator mediator;

    public AssignmentsController(IMediator mediator)
    {
        this.mediator = mediator;
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
}
