using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Application.Options;

public sealed class AzureBlobStorage : IFileStorage
{
    private readonly BlobContainerClient container;

    public AzureBlobStorage(IOptions<FileStorageOptions> opts)
    {
        var o = opts.Value;
        //container = new BlobContainerClient(o.AzureConnectionString, o.AzureContainer);
        container.CreateIfNotExists();
    }

    public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct)
    {
        var blob = container.GetBlobClient(relativePath);
        await blob.UploadAsync(content, overwrite: true, ct);
        return blob.Uri.ToString(); // вже повний URL
    }

    public async Task DeleteAsync(string relative, CancellationToken ct)
    {
        var blob = container.GetBlobClient(relative);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task<Stream> OpenReadAsync(string relative, CancellationToken ct)
    {
        var blob = container.GetBlobClient(relative);
        var download = await blob.DownloadAsync(ct);
        return download.Value.Content;
    }
}
