using RecamNewBackend.Common.Enums;

namespace RecamNewBackend.DTOs.ListingCase;

public class ListingCaseQueryDto
{
    public PropertyStatus? Status { get; set; }
    public PropertyType? PropertyType { get; set; }
    public SaleType? SaleType { get; set; }
    public string? Address { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
