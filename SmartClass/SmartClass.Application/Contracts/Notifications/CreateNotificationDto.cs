using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Notifications
{
    /// <summary>
    /// Те, що потрібне для створення нотифікації на бекенді.
    /// </summary>
    public sealed record CreateNotificationDto(
        Guid UserId,
        string Type,
        string PayloadJson
    );
}
