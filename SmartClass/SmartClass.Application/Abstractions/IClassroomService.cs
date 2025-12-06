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

    }
}
