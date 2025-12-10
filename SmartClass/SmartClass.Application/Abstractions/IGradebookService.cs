using SmartClass.Application.Contracts.Gradebook;

namespace SmartClass.Application.Abstractions
{
    public interface IGradebookService
    {
        /// <summary>
        /// Повний журнал оцінок по класу (усі студенти × усі завдання).
        /// </summary>
        Task<GradebookDto> GetForClassroomAsync(
            Guid classroomId,
            CancellationToken ct = default);
    }
}
