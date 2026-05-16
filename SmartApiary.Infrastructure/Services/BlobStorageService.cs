using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SmartApiary.Application.Interfaces;

namespace SmartApiary.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;

    public BlobStorageService(IConfiguration config, IHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string containerName)
    {
        var blobName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
        return await UploadAsync(imageStream, containerName, blobName);
    }

    public async Task<string> UploadThumbnailAsync(Stream imageStream, string fileName)
    {
        var container = _config["BlobStorage:ApiaryThumbnailsContainer"] ?? "apiary-thumbnails";
        await using var thumbStream = new MemoryStream();
        using (var image = await Image.LoadAsync(imageStream))
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(200, 200)
            }));
            await image.SaveAsJpegAsync(thumbStream);
        }
        thumbStream.Position = 0;
        var blobName = $"{Guid.NewGuid():N}_thumb_{SanitizeFileName(fileName)}.jpg";
        return await UploadAsync(thumbStream, container, blobName);
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;

        if (UseLocalStorage())
        {
            var localPath = UrlToLocalPath(imageUrl);
            if (localPath != null && File.Exists(localPath))
                File.Delete(localPath);
            return;
        }

        if (!TryParseBlobUrl(imageUrl, out var container, out var blobName))
            return;

        var client = GetBlobServiceClient().GetBlobContainerClient(container);
        await client.GetBlobClient(blobName).DeleteIfExistsAsync();
    }

    private async Task<string> UploadAsync(Stream stream, string containerName, string blobName)
    {
        if (UseLocalStorage())
            return await UploadLocalAsync(stream, containerName, blobName);

        var container = GetBlobServiceClient().GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        var blob = container.GetBlobClient(blobName);
        stream.Position = 0;
        await blob.UploadAsync(stream, overwrite: true);
        return blob.Uri.ToString();
    }

    private async Task<string> UploadLocalAsync(Stream stream, string containerName, string blobName)
    {
        var root = GetLocalRoot();
        var dir = Path.Combine(root, containerName);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, blobName);
        await using var file = File.Create(path);
        stream.Position = 0;
        await stream.CopyToAsync(file);
        var publicBase = _config["BlobStorage:PublicBaseUrl"]?.TrimEnd('/') ?? "http://localhost:7014/media";
        return $"{publicBase}/{containerName}/{blobName}";
    }

    private bool UseLocalStorage()
    {
        var mode = _config["BlobStorage:Mode"];
        if (string.Equals(mode, "Local", StringComparison.OrdinalIgnoreCase))
            return true;
        var conn = _config["BlobStorage:ConnectionString"];
        return string.IsNullOrWhiteSpace(conn) ||
               string.Equals(conn, "Local", StringComparison.OrdinalIgnoreCase);
    }

    private string GetLocalRoot()
    {
        var configured = _config["BlobStorage:LocalRoot"];
        if (!string.IsNullOrWhiteSpace(configured))
            return Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(_env.ContentRootPath, configured);
        return Path.Combine(_env.ContentRootPath, "wwwroot", "media");
    }

    private BlobServiceClient GetBlobServiceClient()
    {
        var connectionString = _config["BlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("BlobStorage:ConnectionString nije podešen.");
        return new BlobServiceClient(connectionString);
    }

    private string? UrlToLocalPath(string url)
    {
        var publicBase = _config["BlobStorage:PublicBaseUrl"]?.TrimEnd('/') ?? "http://localhost:7014/media";
        if (!url.StartsWith(publicBase, StringComparison.OrdinalIgnoreCase))
            return null;
        var relative = url[publicBase.Length..].TrimStart('/');
        return Path.Combine(GetLocalRoot(), relative.Replace('/', Path.DirectorySeparatorChar));
    }

    private static bool TryParseBlobUrl(string url, out string container, out string blobName)
    {
        container = "";
        blobName = "";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        var segments = uri.AbsolutePath.Trim('/').Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2) return false;
        container = segments[0];
        blobName = segments[1];
        return true;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "image.jpg" : name;
    }
}
