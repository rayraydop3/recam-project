using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RecamNewBackend.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration["Azure:BlobStorage:ConnectionString"];
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(IFormFile file, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        
        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);

        await containerClient.DeleteBlobIfExistsAsync(blobName);
    }

    public async Task<Stream> DownloadAsync(string blobUrl, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
        var blobClient = containerClient.GetBlobClient(blobName);

        var download = await blobClient.DownloadStreamingAsync();
        return download.Value.Content;
    }
}
