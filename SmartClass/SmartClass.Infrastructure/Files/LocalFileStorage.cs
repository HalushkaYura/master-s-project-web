using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment env;
    private readonly string basePath;

    public LocalFileStorage(IWebHostEnvironment env, IOptions<FileStorageOptions> options)
    {
        this.env = env;
        basePath = options.Value.BasePath ?? "uploads";
    }

    public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct = default)
    {
        var root = Path.Combine(env.WebRootPath ?? "wwwroot", basePath);
        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, ct);
        }

        // повертаємо відносний шлях типу "uploads/classroom/..."
        return Path.Combine(basePath, relativePath).Replace('\\', '/');
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var root = env.WebRootPath ?? "wwwroot";
        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var root = env.WebRootPath ?? "wwwroot";
        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Stream s = File.OpenRead(fullPath);
        return Task.FromResult(s);
    }
}

public sealed class FileStorageOptions
{
    public string Provider { get; set; } = "Local";
    public string BasePath { get; set; } = "uploads";
}
