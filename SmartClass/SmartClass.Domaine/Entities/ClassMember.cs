namespace SmartClass.Domain.Entities
{
    public class ClassMember : BaseEntity
    {
        public Guid ClassroomId { get; set; }
        public Guid UserId { get; set; }

        public string RoleInClass { get; set; } = "Student"; // зберігаємо як string

        public Classroom Classroom { get; set; } = null!;
    }
}
