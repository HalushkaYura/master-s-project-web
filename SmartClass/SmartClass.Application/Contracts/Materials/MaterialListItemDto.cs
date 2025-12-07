using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Materials
{
    public sealed record MaterialListItemDto(
        Guid Id,
        string Title,
        string? Description,
            DateTime CreatedAt,

        IReadOnlyList<FileResourceDto> Files
    );

}
