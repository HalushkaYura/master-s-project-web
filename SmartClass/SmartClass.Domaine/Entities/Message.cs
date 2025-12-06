namespace SmartClass.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid ChannelId { get; set; }
        public Guid AuthorId { get; set; }          // ApplicationUser.Id
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public Channel Channel { get; set; } = null!;
    }
}
