using RecamNewBackend.DTOs.PhotographyCompany;

namespace RecamNewBackend.Services;

public interface IPhotographyCompanyService
{
    Task<List<PhotographyCompanyDto>> GetAllAsync();
}