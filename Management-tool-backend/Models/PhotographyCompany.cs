namespace RecamNewBackend.Models;

public class PhotographyCompany: User
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
    public ICollection<ListingCase> ListingCases { get; set; } = new List<ListingCase>();
}