using SmartClass.Application.Contracts.Classrooms;

namespace SmartClass.Application.Abstractions
{
    public interface IClassroomService
    {
        /// <summary>
        /// Створення класу викладачем.
        /// </summary>
        Task<ClassroomDto> CreateClassroomAsync(CreateClassroomDto dto, Guid ownerId, CancellationToken ct = default);

        /// <summary>
        /// Приєднання користувача за кодом.
        /// </summary>
        Task JoinClassroomAsync(string joinCode, Guid ApplicationUserId, CancellationToken ct = default);

        /// <summary>
        /// Отримати всі класи, де користувач є учасником (і роль).
        /// </summary>
        Task<IReadOnlyList<MyClassroomDto>> GetMyClassroomsAsync(Guid ApplicationUserId, CancellationToken ct = default);

        /// <summary>
        /// Отримати всю інформацію, про клас , включно з учасниками.
        /// </summary>
        Task<ClassroomDetailsDto> GetClassroomDetailsAsync(
    Guid classroomId,
    Guid currentUserId,
    CancellationToken ct = default);
        /// <summary>
        /// Отримати інформацію про клас за кодом приєднання.
        /// </summary>
        Task<ClassroomDetailsDto> GetClassroomByJoinCodeAsync(
    string joinCode,
    CancellationToken ct = default);

        /// <summary>
        /// Оновлення інформації про клас викладачем.
        /// </summary>
        Task UpdateAsync(UpdateClassroomDto dto, Guid ownerId, CancellationToken ct = default);

        /// <summary>
        /// Видалення класу викладачем.
        /// </summary>
        Task DeleteAsync(Guid classroomId, Guid ownerId, CancellationToken ct = default);
    }
}
