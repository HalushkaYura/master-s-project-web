using SmartClass.Application.Contracts;
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
        Task<AssignmentShortDto> CreateAsync(CreateAssignmentDto dto, Guid teacherId, CancellationToken ct = default);
        Task<IReadOnlyList<AssignmentShortDto>> GetForClassroomAsync(Guid classroomId, CancellationToken ct = default);
        Task<AssignmentDetailsDto?> GetByIdAsync(Guid classroomId, Guid assignmentId, CancellationToken ct = default);

        Task UpdateAsync(UpdateAssignmentDto dto, Guid teacherId, CancellationToken ct = default);
        Task DeleteAsync(Guid assignmentId, Guid teacherId, CancellationToken ct = default);

        Task<FileResourceDto> UploadAttachmentAsync(
      Guid classroomId,
      Guid assignmentId,
      Guid ownerId,
      string fileName,
      string contentType,
      long sizeBytes,
      Stream content,
      CancellationToken ct = default);
    }
}
