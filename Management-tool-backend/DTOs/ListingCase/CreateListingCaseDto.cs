using RecamNewBackend.Common.Enums;

namespace RecamNewBackend.DTOs.ListingCase;

public class CreateListingCaseDto
{
    public string Address { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public SaleType SaleType { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Garages { get; set; }
    public double LandSize { get; set; }
}
