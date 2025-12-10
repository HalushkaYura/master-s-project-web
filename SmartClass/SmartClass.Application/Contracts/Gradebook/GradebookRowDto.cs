using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Gradebook
{
    public sealed record GradebookRowDto(
         Guid StudentId,
         string StudentName,
         IReadOnlyList<GradebookCellDto> Cells
     );
}
