using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Features.Classrooms.Commands.Create;

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
}
