using RecamNewBackend.Common.Enums;

namespace RecamNewBackend.Models;

public class ListingCase
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public PropertyStatus Status { get; set; } = PropertyStatus.Created;
    public PropertyType PropertyType { get; set; }
    public SaleType SaleType { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Garages { get; set; }
    public double LandSize { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ShareToken { get; set; }

    public string PhotographyCompanyId { get; set; } = string.Empty;
    public PhotographyCompany PhotographyCompany { get; set; } = null!;

    public string? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    public ICollection<CaseContact> CaseContacts { get; set; } = new List<CaseContact>();
    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}