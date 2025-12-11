using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Materials
{
    public record UpdateMaterialDto(
        Guid Id,
        string Title,
        string? Description,
        string? ContentHtml
    );
}
