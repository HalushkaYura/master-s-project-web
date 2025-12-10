using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Gradebook
{
    public record GradebookAssignmentDto(
        Guid AssignmentId,
        string Title,
        int MaxPoints,
        DateTime? DueAt,
        string Status // Draft / Published / Closed
    );
}
