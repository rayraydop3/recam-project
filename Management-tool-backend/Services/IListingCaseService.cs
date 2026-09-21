using RecamNewBackend.Common;
using RecamNewBackend.DTOs.ListingCase;

namespace RecamNewBackend.Services;

public interface IListingCaseService
{
    Task<ListingCaseDto> CreateAsync(CreateListingCaseDto dto, string companyId);
    Task<PagedResult<ListingCaseDto>> GetAllAsync(string companyId, ListingCaseQueryDto query);
    Task<ListingCaseDto> UpdateAsync(int id, UpdateListingCaseDto dto, string companyId);
    Task DeleteAsync(int id, string companyId); 
    Task<ListingCaseDto> GetByIdAsync(int id, string companyId); 
    Task<ListingCaseDto> ChangeStatusAsync(int id, ChangeStatusDto dto, string userId);
    Task<string> GenerateShareTokenAsync(int id, string companyId);
    Task<ShareableLinkDto> GetByShareTokenAsync(string token);
    Task<ListingCaseDto> AssignAgentAsync(int id, AssignAgentDto dto, string companyId);
}