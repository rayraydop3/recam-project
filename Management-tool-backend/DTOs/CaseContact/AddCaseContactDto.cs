namespace RecamNewBackend.DTOs.CaseContact;

public class AddCaseContactDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? ProfileImageUrl { get; set; }
}
