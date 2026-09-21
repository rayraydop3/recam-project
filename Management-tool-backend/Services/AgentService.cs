using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.Agent;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Models;

namespace RecamNewBackend.Services;

public class AgentService : IAgentService
{
    private readonly RecamDbContext _context;
    private readonly UserManager<User> _userManager;

    public AgentService(RecamDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AgentDto> CreateAsync(AddAgentDto dto, string companyId)
    {
        var agent = new Agent
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.Phone,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            ProfileImageUrl = dto.ProfileImageUrl,
            PhotographyCompanyId = companyId
        };

        var result = await _userManager.CreateAsync(agent, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception(errors);
        }

        await _userManager.AddToRoleAsync(agent, "Agent");

        return MapToDto(agent);
    }

    public async Task<List<AgentDto>> GetAllAsync()
    {
        var agents = await _context.Agents
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        return agents.Select(MapToDto).ToList();
    }

    public async Task<List<AgentDto>> GetByCompanyAsync(string companyId)
    {
        var agents = await _context.Agents
            .Where(a => a.PhotographyCompanyId == companyId && !a.IsDeleted)
            .ToListAsync();

        return agents.Select(MapToDto).ToList();
    }

    public async Task<AgentDto?> SearchByEmailAsync(string email)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Email == email && !a.IsDeleted);

        return agent == null ? null : MapToDto(agent);
    }

    private static AgentDto MapToDto(Agent a) => new AgentDto
    {
        Id = a.Id,
        FirstName = a.FirstName,
        LastName = a.LastName,
        Email = a.Email!,
        Phone = a.PhoneNumber,
        ProfileImageUrl = a.ProfileImageUrl,
        PhotographyCompanyId = a.PhotographyCompanyId,
        CreatedAt = a.CreatedAt
    };
}
