namespace SmartClass.Domain.Entities
{
    public class ChannelMember : BaseEntity
    {
        public Guid ChannelId { get; set; }
        public Guid UserId { get; set; }
        public bool IsMuted { get; set; }

        public Channel Channel { get; set; } = null!;
    }
}
