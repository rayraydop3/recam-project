namespace RecamNewBackend.Models;

public class MediaSelectionHistory
{
    public int Id { get; set; }
    public bool IsSelected { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public int ListingCaseId { get; set; }
    public ListingCase ListingCase { get; set; } = null!;

    public int MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;

    public string AgentId { get; set; } = string.Empty;
    public Agent Agent { get; set; } = null!;
}
