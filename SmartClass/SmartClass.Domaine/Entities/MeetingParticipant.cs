namespace SmartClass.Domain.Entities
{
    public class MeetingParticipant : BaseEntity
    {
        public Guid MeetingId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "Attendee";  // Host|Presenter|Attendee
        public DateTime? JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
    }
}
