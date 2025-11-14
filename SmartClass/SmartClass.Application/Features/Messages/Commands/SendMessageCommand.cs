using MediatR;
using SmartClass.Application.Contracts.Messages;

namespace SmartClass.Application.Features.Messages.Commands;

public sealed record SendMessageCommand(
    Guid ChannelId,
    Guid? AuthorId,
    string Text
) : IRequest<ChatMessageDto>;
