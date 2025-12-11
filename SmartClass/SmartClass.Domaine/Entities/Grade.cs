namespace SmartClass.Domain.Entities
{
    public class Grade : BaseEntity
    {
        public Guid SubmissionId { get; set; }     // Звʼязок з відповіддю студента
        public Submission Submission { get; set; } = null!;

        public decimal Score { get; set; }         // Оцінка (наприклад 0–100)
        public string? Comment { get; set; }       // Коментар викладача

        public DateTime GradedAt { get; set; }     // Дата оцінювання
        public Guid GradedBy { get; set; }         // Id викладача
    }
}
