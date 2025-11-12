using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Features.Grades.Commands.GradeSubmission;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public sealed class GradesController : ControllerBase
{
    private readonly IMediator mediator;
    public GradesController(IMediator mediator) => this.mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Grade([FromBody] GradeSubmissionCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { gradeId = id });
    }
}
