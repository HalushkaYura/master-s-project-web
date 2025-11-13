using System.Threading;
using System.Threading.Tasks;

namespace SmartClass.Application.Abstractions;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string type, string payloadJson, CancellationToken ct = default);
}
