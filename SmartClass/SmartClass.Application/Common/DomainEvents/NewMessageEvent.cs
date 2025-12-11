using MediatR;

namespace SmartClass.Application.Common.DomainEvents
{
    public sealed class NewMessageEvent : INotification
    {
        public Guid ChannelId { get; }
        public Guid MessageId { get; }
        public Guid AuthorId { get; }
        public string Text { get; }

        public NewMessageEvent(Guid channelId, Guid messageId, Guid authorId, string text)
        {
            ChannelId = channelId;
            MessageId = messageId;
            AuthorId = authorId;
            Text = text;
        }
    }
}
