using RecamNewBackend.Common.Enums;

namespace RecamNewBackend.Models;

public class MediaAsset
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public bool IsCoverImage { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ListingCaseId { get; set; }
    public ListingCase ListingCase { get; set; } = null!;

    public ICollection<SelectedMedia> SelectedMedias { get; set; } = new List<SelectedMedia>();
}