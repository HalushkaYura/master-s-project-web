using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public sealed class GradesController : ControllerBase
{
    private readonly IMediator mediator;
    public GradesController(IMediator mediator) => this.mediator = mediator;


}
