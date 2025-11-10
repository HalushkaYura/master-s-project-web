using SmartClass.Domain.Enums;
using SmartClass.Domaine.Primitives;

namespace SmartClass.Domain.Entities
{
    public class ClassMember : BaseEntity
    {
        public Guid ClassroomId { get; set; }
        public Guid UserId { get; set; }             // ApplicationUser.Id
        public ClassRole RoleInClass { get; set; }


        public Classroom Classroom { get; set; } = null!;
        // Якщо маєш користувача — теж додай навігацію:
        // public ApplicationUser User { get; set; } = null!;
    }
}
