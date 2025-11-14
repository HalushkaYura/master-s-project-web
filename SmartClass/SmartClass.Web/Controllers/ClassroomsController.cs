using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Features.Classrooms.Commands.Create;
using SmartClass.Application.Features.Classrooms.Commands.Join;
using SmartClass.Application.Features.Classrooms.Create;
using SmartClass.Application.Features.Classrooms.Queries.GetMyClassrooms;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public sealed class ClassroomsController : ControllerBase
{
    private readonly IMediator mediator;

    public ClassroomsController(IMediator mediator) => this.mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassroomCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { classroomId = id });
    }

    [HttpPost("join")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Join([FromBody] JoinClassroomCommand command, CancellationToken ct)
    {
        var classroomId = await mediator.Send(command, ct);
        return Ok(new { classroomId });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyClassroomsQuery(), ct);
        return Ok(result);
    }
}
