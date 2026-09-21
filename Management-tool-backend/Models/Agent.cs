namespace RecamNewBackend.Models;

public class Agent: User
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string PhotographyCompanyId { get; set; } = string.Empty;
    public PhotographyCompany PhotographyCompany { get; set; } = null!;

    public ICollection<ListingCase> ListingCases { get; set; } = new List<ListingCase>();
}