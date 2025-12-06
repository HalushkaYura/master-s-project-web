using SmartClass.Domain.Enums;

namespace SmartClass.Domain.Entities
{
    public class Channel : BaseEntity
    {
        public Guid ClassroomId { get; set; }                // 🔹 зробив not-null
        public ChannelType Type { get; set; }
        public string Title { get; set; } = string.Empty;    // 🔹 теж not-null

        public Classroom Classroom { get; set; } = null!;
        public ICollection<ChannelMember> Members { get; set; } = new List<ChannelMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
