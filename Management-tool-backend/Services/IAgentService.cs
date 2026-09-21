using RecamNewBackend.DTOs.Agent;

namespace RecamNewBackend.Services;

public interface IAgentService
{
    Task<AgentDto> CreateAsync(AddAgentDto dto, string companyId);
    Task<List<AgentDto>> GetAllAsync();
    Task<List<AgentDto>> GetByCompanyAsync(string companyId);
    Task<AgentDto?> SearchByEmailAsync(string email);
}
