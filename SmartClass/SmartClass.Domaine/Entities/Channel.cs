using SmartClass.Domain.Enums;
using SmartClass.Domaine.Primitives;

namespace SmartClass.Domain.Entities
{
    public class Channel : BaseEntity
    {
        public Guid? ClassroomId { get; set; }
        public ChannelType Type { get; set; }
        public string? Title { get; set; }
    }
}
