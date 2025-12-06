using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Features.Messages.Commands;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ICurrentUser currentApplicationUser;

        public MessagesController(IMediator mediator, ICurrentUser currentApplicationUser)
        {
            this.mediator = mediator;
            this.currentApplicationUser = currentApplicationUser;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatMessageDto>> Send([FromBody] SendMessageDto dto, CancellationToken ct)
        {
            if (currentApplicationUser.UserId == null)
                return Unauthorized("ApplicationUser is not authenticated.");

            var result = await mediator.Send(new SendMessageCommand(
                dto.ChannelId,
                currentApplicationUser.UserId.Value,
                dto.Text
            ), ct);

            return Ok(result);
        }
    }

    public sealed class SendMessageDto
    {
        public Guid ChannelId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
