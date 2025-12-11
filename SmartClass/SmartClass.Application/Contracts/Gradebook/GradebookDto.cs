using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Gradebook
{
    public record GradebookDto(
        Guid ClassroomId,
        string ClassroomTitle,
        IReadOnlyList<GradebookAssignmentDto> Assignments,
        IReadOnlyList<GradebookRowDto> Rows
    );
}
