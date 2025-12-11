using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Classrooms
{
    public record CreateMaterialDto(
        Guid ClassroomId,
        string Title,
        string? Description,
        string? ContentHtml
    );
}
