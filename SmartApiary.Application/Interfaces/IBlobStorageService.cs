namespace SmartApiary.Application.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string containerName);
    Task<string> UploadThumbnailAsync(Stream imageStream, string fileName);
    Task DeleteImageAsync(string imageUrl);
}
