namespace RecamNewBackend.DTOs.SelectedMedia;

public class MediaSelectionHistoryDto
{
    public int Id { get; set; }
    public int MediaAssetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public DateTime ChangedAt { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
}
