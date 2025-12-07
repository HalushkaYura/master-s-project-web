namespace SmartClass.Domain.Entities
{
    public class Classroom : BaseEntity
    {
        public Guid OwnerId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }

        public ICollection<ClassMember> Members { get; set; } = new List<ClassMember>();
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();

        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
