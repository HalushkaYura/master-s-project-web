using SmartClass.Domaine.Primitives;

namespace SmartClass.Domain.Entities
{
    public class Classroom : BaseEntity
    {
        public Guid OwnerId { get; set; }            // ApplicationUser.Id
        public string Title { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }

        public ICollection<ClassMember> Members { get; set; } = new List<ClassMember>();
    }
}
