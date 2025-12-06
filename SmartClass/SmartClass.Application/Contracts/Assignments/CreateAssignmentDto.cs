using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Assignments
{
    public record CreateAssignmentDto(
        Guid ClassroomId,
        string Title,
        string? DescriptionHtml,
        int PointsMax,
        DateTime? DueAt,
        bool AllowLate
    );
}
