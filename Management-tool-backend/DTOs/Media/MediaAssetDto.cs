namespace RecamNewBackend.DTOs.Media;

public class MediaAssetDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public bool IsCoverImage { get; set; }
    public DateTime CreatedAt { get; set; }
}
