namespace SmartClass.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = "{}";
        public bool IsRead { get; set; }
    }
}
