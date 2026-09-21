namespace RecamNewBackend.Services;

public interface IBlobService
{
    Task<string> UploadAsync(IFormFile file, string containerName);
    Task DeleteAsync(string blobUrl, string containerName);
    Task<Stream> DownloadAsync(string blobUrl, string containerName);
}
