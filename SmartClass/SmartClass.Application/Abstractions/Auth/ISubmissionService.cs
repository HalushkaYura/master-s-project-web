using SmartClass.Application.Contracts.Submissions;

namespace SmartClass.Application.Abstractions
{
    public interface ISubmissionService
    {
        /// <summary>
        /// Для студента: гарантує, що сабміт існує, і повертає його.
        /// Якщо записи немає – створює NotSubmitted.
        /// </summary>
        Task<SubmissionDetailsDto> EnsureForStudentAsync(
            Guid assignmentId,
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Для студента / викладача: отримати деталі сабміту.
        /// </summary>
        Task<SubmissionDetailsDto?> GetByIdAsync(Guid submissionId, CancellationToken ct = default);

        /// <summary>
        /// Для викладача: список сабмітів по завданню.
        /// </summary>
        Task<IReadOnlyList<SubmissionListItemDto>> GetForAssignmentAsync(
            Guid assignmentId,
            CancellationToken ct = default);

        /// <summary>
        /// Для викладача: виставити / оновити оцінку.
        /// </summary>
        Task GradeAsync(GradeSubmissionDto dto, Guid teacherId, CancellationToken ct = default);
    }
}
