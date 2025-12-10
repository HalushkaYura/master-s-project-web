using SmartClass.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Gradebook
{
    public record GradebookCellDto(
        Guid AssignmentId,
        Guid? SubmissionId,
        SubmissionStatus Status,
        DateTime? SubmittedAt,
        decimal? Score,
        string? Comment,
        int MaxPoints
    );
}
