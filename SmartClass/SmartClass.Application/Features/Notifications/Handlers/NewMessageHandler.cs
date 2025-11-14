using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using SmartClass.Application.Contracts.Messages;
using SmartClass.Domain.Entities;
using System.Text.Json;

namespace SmartClass.Application.Features.Notifications.Handlers;

public sealed class NewMessageHandler : INotificationHandler<NewMessageEvent>
{
    private readonly IRepository<ChannelMember> channelMemberRepository;
    private readonly INotificationService notificationService;
    private readonly IChatRealtimeSender chatRealtimeSender;

    public NewMessageHandler(
        IRepository<ChannelMember> channelMemberRepository,
        INotificationService notificationService,
        IChatRealtimeSender chatRealtimeSender)
    {
        this.channelMemberRepository = channelMemberRepository;
        this.notificationService = notificationService;
        this.chatRealtimeSender = chatRealtimeSender;
    }

    public async Task Handle(NewMessageEvent e, CancellationToken ct)
    {
        var members = await channelMemberRepository.GetListAsync(m => m.ChannelId == e.ChannelId);

        // 1) Нотифікації всім, крім автора
        var payload = JsonSerializer.Serialize(new
        {
            channelId = e.ChannelId,
            messageId = e.MessageId,
            text = e.Text
        });

        foreach (var m in members.Where(x => x.UserId != e.AuthorId))
        {
            await notificationService.CreateAsync(
                userId: m.UserId,
                type: "NewMessage",
                payloadJson: payload,
                ct);
        }

        // 2) Live SignalR повідомлення в канал
        var dto = new ChatMessageDto
        {
            Id = e.MessageId,
            ChannelId = e.ChannelId,
            AuthorId = e.AuthorId,
            Text = e.Text,
            CreatedAt = DateTime.UtcNow,
            AuthorName = "" // можеш додати витяг з user service
        };

        await chatRealtimeSender.SendToChannelAsync(e.ChannelId, dto, ct);
    }
}
