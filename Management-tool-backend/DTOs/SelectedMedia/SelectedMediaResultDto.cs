namespace RecamNewBackend.DTOs.SelectedMedia;

public class SelectedMediaResultDto
{
    public int Id { get; set; }
    public int MediaAssetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public bool IsCoverImage { get; set; }
    public bool IsSelected { get; set; }
    public DateTime SelectedAt { get; set; }
}
