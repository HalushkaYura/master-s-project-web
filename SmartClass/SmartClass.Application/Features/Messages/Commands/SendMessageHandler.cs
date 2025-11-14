using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using SmartClass.Application.Contracts.Messages;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Messages.Commands;

public sealed class SendMessageHandler
    : IRequestHandler<SendMessageCommand, ChatMessageDto>
{
    private readonly IRepository<Message> messageRepository;
    private readonly ICurrentUserService currentUser;
    private readonly IMediator mediator;

    public SendMessageHandler(
        IRepository<Message> messageRepository,
        ICurrentUserService currentUser,
        IMediator mediator)
    {
        this.messageRepository = messageRepository;
        this.currentUser = currentUser;
        this.mediator = mediator;
    }

    public async Task<ChatMessageDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var entity = new Message
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            AuthorId = userId,
            Text = request.Text
        };

        await messageRepository.AddAsync(entity);
        await messageRepository.SaveChangesAsync();

        // Подія для нотифікацій і реального часу
        await mediator.Publish(new NewMessageEvent(
            channelId: entity.ChannelId,
            messageId: entity.Id,
            authorId: entity.AuthorId,
            text: entity.Text
        ), ct);

        return new ChatMessageDto
        {
            Id = entity.Id,
            ChannelId = entity.ChannelId,
            AuthorId = entity.AuthorId,
            Text = entity.Text,
            CreatedAt = entity.CreatedAt,
            AuthorName = currentUser.Name ?? "User"
        };
    }
}
