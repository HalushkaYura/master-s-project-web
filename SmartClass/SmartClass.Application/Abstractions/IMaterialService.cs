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
        Task UpdateAsync(UpdateMaterialDto dto, Guid teacherId, CancellationToken ct = default);
        Task DeleteAsync(Guid materialId, Guid teacherId, CancellationToken ct = default);
    }
}
