using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Options;

namespace SmartClass.Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string webRoot;
    private readonly FileStorageOptions options;

    public LocalFileStorage(IWebHostEnvironment env, IOptions<FileStorageOptions> opts)
    {
        this.options = opts.Value;
        webRoot = env.WebRootPath!;
        Directory.CreateDirectory(Path.Combine(webRoot, options.BasePath));
    }

    public async Task<string> SaveAsync(Stream content, string relative, CancellationToken ct)
    {
        var nextPath = Path.Combine(options.BasePath, relative);
        var full = Path.Combine(webRoot, nextPath);

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        using var fs = new FileStream(full, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fs, ct);

        return "/" + nextPath.Replace("\\", "/");
    }

    public Task DeleteAsync(string relative, CancellationToken ct)
    {
        var full = Path.Combine(webRoot, relative.TrimStart('/').Replace("/", "\\"));
        if (File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string relative, CancellationToken ct)
    {
        var full = Path.Combine(webRoot, relative.TrimStart('/').Replace("/", "\\"));
        if (!File.Exists(full)) throw new FileNotFoundException();

        Stream s = new FileStream(full, FileMode.Open, FileAccess.Read);
        return Task.FromResult(s);
    }
}
