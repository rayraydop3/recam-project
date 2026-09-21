using RecamNewBackend.Common.Enums;

namespace RecamNewBackend.Models;

public class StatusHistory
{
    public int Id { get; set; }
    public PropertyStatus OldStatus { get; set; }
    public PropertyStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public int ListingCaseId { get; set; }
    public ListingCase ListingCase { get; set; } = null!;

    public string ChangedByUserId { get; set; } = string.Empty;
    public User ChangedByUser { get; set; } = null!;
}