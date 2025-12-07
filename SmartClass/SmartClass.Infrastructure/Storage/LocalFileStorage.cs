using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions.Storage;

namespace SmartClass.Infrastructure.Storage
{
    public sealed class FileStorageOptions
    {
        public string Provider { get; set; } = "Local";
        public string BasePath { get; set; } = "uploads";
        public int MaxUploadMb { get; set; } = 100;
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    }

    public sealed class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment env;
        private readonly FileStorageOptions options;

        public LocalFileStorage(IWebHostEnvironment env, IOptions<FileStorageOptions> options)
        {
            this.env = env;
            this.options = options.Value;
        }

        private string GetPhysicalPath(string relativePath)
        {
            // base = {webroot}/uploads
            var basePath = Path.Combine(env.WebRootPath, options.BasePath);
            return Path.Combine(basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct = default)
        {
            var physicalPath = GetPhysicalPath(relativePath);

            var dir = Path.GetDirectoryName(physicalPath)!;
            Directory.CreateDirectory(dir);

            using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(fileStream, ct);
            }

            // в БД зберігаємо відносний шлях від "uploads"
            return relativePath.Replace('\\', '/');
        }

        public Task DeleteAsync(string relativePath, CancellationToken ct = default)
        {
            var physicalPath = GetPhysicalPath(relativePath);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default)
        {
            var physicalPath = GetPhysicalPath(relativePath);
            if (!File.Exists(physicalPath))
                throw new FileNotFoundException("File not found", physicalPath);

            Stream s = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult(s);
        }
    }
}
