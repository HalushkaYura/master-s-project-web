using SmartClass.Application.Contracts.Assignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Abstractions
{
    public interface IAssignmentService
    {
        Task<AssignmentDto> CreateAsync(CreateAssignmentDto dto, Guid teacherId, CancellationToken ct = default);
        Task<IReadOnlyList<AssignmentDto>> GetForClassroomAsync(Guid classroomId, CancellationToken ct = default);
        Task UpdateAsync(UpdateAssignmentDto dto, Guid teacherId, CancellationToken ct = default);
        Task DeleteAsync(Guid assignmentId, Guid teacherId, CancellationToken ct = default);
    }
}
