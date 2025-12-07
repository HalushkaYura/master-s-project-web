using SmartClass.Application.Contracts;
using SmartClass.Application.Contracts.Classrooms;
using SmartClass.Application.Contracts.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Abstractions
{
    public interface IMaterialService
    {
        Task<MaterialDto> CreateAsync(CreateMaterialDto dto, Guid teacherId, CancellationToken ct = default);
        Task<IReadOnlyList<MaterialDto>> GetForClassroomAsync(Guid classroomId, CancellationToken ct = default);
        Task<MaterialDto?> GetByIdAsync(Guid classroomId, Guid materialId, CancellationToken ct = default); // 🔹 ДОДАЛИ
        Task UpdateAsync(UpdateMaterialDto dto, Guid teacherId, CancellationToken ct = default);
        Task DeleteAsync(Guid materialId, Guid teacherId, CancellationToken ct = default);

        Task<FileResourceDto> UploadFileAsync(
            Guid classroomId,
            Guid materialId,
            Guid ownerId,
            string fileName,
            string contentType,
            long sizeBytes,
            Stream content,
            CancellationToken ct = default);
    }

}
