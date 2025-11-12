using Microsoft.AspNetCore.Hosting;
using SmartClass.Application.Abstractions.Storage;

namespace SmartClass.Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string webRoot;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(webRoot);
    }

    public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(webRoot, Normalize(relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct);
        return Normalize(relativePath).Replace('\\', '/'); // BlobPath як /uploads/...
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(webRoot, Normalize(relativePath));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(webRoot, Normalize(relativePath));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found", fullPath);

        Stream s = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(s);
    }

    private static string Normalize(string path)
    {
        path = path.Replace('/', Path.DirectorySeparatorChar);
        if (path.StartsWith(Path.DirectorySeparatorChar)) path = path[1..];
        return path;
    }
}
