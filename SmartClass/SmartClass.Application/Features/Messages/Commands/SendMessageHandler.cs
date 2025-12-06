using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Messages.Commands
{
    public sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, ChatMessageDto>
    {
        private readonly IRepository<Message> messages;
        private readonly IMediator mediator;

        public SendMessageHandler(
            IRepository<Message> messages,
            IMediator mediator)
        {
            this.messages = messages;
            this.mediator = mediator;
        }

        public async Task<ChatMessageDto> Handle(SendMessageCommand req, CancellationToken ct)
        {
            var msg = new Message
            {
                ChannelId = req.ChannelId,
                AuthorId = req.AuthorId,
                Text = req.Text
            };

            await messages.AddAsync(msg);
            await messages.SaveChangesAsync();

            await mediator.Publish(new NewMessageEvent(
                msg.ChannelId, msg.Id, msg.AuthorId, msg.Text
            ), ct);

            return new ChatMessageDto
            {
                Id = msg.Id,
                ChannelId = msg.ChannelId,
                AuthorId = msg.AuthorId,
                Text = msg.Text,
                CreatedAt = msg.CreatedAt,
                AuthorName = "" // при бажанні тут можна теж підтягнути
            };
        }
    }
}
