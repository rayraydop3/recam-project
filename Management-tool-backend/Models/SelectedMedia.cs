namespace RecamNewBackend.Models;

public class SelectedMedia
{
    public int Id { get; set; }
    public bool IsSelected { get; set; } = false;
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;

    public int ListingCaseId { get; set; }
    public ListingCase ListingCase { get; set; } = null!;

    public int MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;

    public string AgentId { get; set; } = string.Empty;
    public Agent Agent { get; set; } = null!;
}