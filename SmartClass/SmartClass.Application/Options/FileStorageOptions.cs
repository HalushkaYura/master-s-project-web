namespace SmartClass.Application.Options;

public sealed class FileStorageOptions
{
    public string Provider { get; set; } = "Local";
    public string BasePath { get; set; } = "uploads";
    public int MaxUploadMb { get; set; } = 100;
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}
