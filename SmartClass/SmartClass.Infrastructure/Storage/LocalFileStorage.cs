using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions.Storage;
using System.IO;

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
        private readonly IWebHostEnvironment _env;
        private readonly FileStorageOptions _options;

        public LocalFileStorage(IWebHostEnvironment env, IOptions<FileStorageOptions> options)
        {
            _env = env;
            _options = options.Value;
        }

        /// <summary>
        /// Перетворює відносний шлях (який зберігаємо в БД) на фізичний:
        /// {webroot}/{BasePath}/{relativePath}
        /// Напр.: wwwroot/uploads/materials/{materialId}/file.ext
        /// </summary>
        private string GetPhysicalPath(string relativePath)
        {
            var basePath = Path.Combine(_env.WebRootPath, _options.BasePath);

            var cleaned = relativePath
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(basePath, cleaned);
        }

        public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct)
        {
            if (content is null)
                throw new ArgumentNullException(nameof(content));

            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("relativePath is required", nameof(relativePath));

            var physicalPath = GetPhysicalPath(relativePath);
            var dir = Path.GetDirectoryName(physicalPath)!;
            Directory.CreateDirectory(dir);

            using (var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(fs, ct);
            }

            // В БД зберігаємо тільки ВІДНОСНИЙ шлях без 'uploads'
            // Напр.: "materials/{materialIdN}/{guid.ext}"
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
            return Task.FromResult<Stream>(s);
        }
    }
}
