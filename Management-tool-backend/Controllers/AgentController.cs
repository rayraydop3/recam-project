using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.DTOs.Agent;
using RecamNewBackend.Services;
using System.Security.Claims;

namespace RecamNewBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _service;

    public AgentController(IAgentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Create a new agent under the current company — Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AgentDto>>> Create([FromBody] AddAgentDto dto)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.CreateAsync(dto, companyId);
        return Ok(ApiResponse<AgentDto>.Success(result, "Agent created successfully"));
    }

    /// <summary>
    /// Get all agents — Admin only
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<AgentDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<AgentDto>>.Success(result, "Agents retrieved successfully"));
    }

    /// <summary>
    /// Get agents belonging to the current photography company
    /// </summary>
    [HttpGet("company")]
    public async Task<ActionResult<ApiResponse<List<AgentDto>>>> GetByCompany()
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetByCompanyAsync(companyId);
        return Ok(ApiResponse<List<AgentDto>>.Success(result, "Agents retrieved successfully"));
    }

    /// <summary>
    /// Search agent by exact email
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<AgentDto?>>> SearchByEmail([FromQuery] string email)
    {
        var result = await _service.SearchByEmailAsync(email);
        return Ok(ApiResponse<AgentDto?>.Success(result, "Search completed"));
    }
}
