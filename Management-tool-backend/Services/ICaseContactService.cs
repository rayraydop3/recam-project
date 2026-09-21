using RecamNewBackend.DTOs.CaseContact;

namespace RecamNewBackend.Services;

public interface ICaseContactService
{
    Task<List<CaseContactDto>> GetByListingCaseIdAsync(int listingCaseId, string companyId);
    Task<CaseContactDto> AddAsync(int listingCaseId, AddCaseContactDto dto, string companyId);
    Task DeleteAsync(int id, string companyId);
}
