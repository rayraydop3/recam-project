namespace RecamNewBackend.DTOs.ListingCase;

public class ListingCaseDto
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string SaleType { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Garages { get; set; }
    public double LandSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PhotographyCompanyId { get; set; } = string.Empty;
    public string? AgentId { get; set; }
}