namespace SmartClass.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid ChannelId { get; set; }
        public Guid AuthorId { get; set; }

        public string Text { get; set; } = string.Empty;

        public Channel Channel { get; set; } = null!;
    }
}
