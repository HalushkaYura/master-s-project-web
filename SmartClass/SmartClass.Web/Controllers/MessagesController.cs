using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Messages;
using SmartClass.Application.Features.Messages.Commands;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ICurrentUserService currentUser;

    public MessagesController(IMediator mediator, ICurrentUserService currentUser)
    {
        this.mediator = mediator;
        this.currentUser = currentUser;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var result = await mediator.Send(new SendMessageCommand(
            dto.ChannelId,
            userId,
            dto.Text
        ));

        return Ok(result);
    }
}
