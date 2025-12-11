using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmartClass.Application.Abstractions.Storage;

public interface IFileStorage
{
    /// <summary>Зберігає файл у вказаний відносний шлях (без wwwroot), повертає відносний шлях (BlobPath).</summary>
    Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct = default);

    /// <summary>Видаляє файл за відносним шляхом (BlobPath).</summary>
    Task DeleteAsync(string relativePath, CancellationToken ct = default);

    /// <summary>Повертає Stream для читання (для завантаження користувачу).</summary>
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default);
}
