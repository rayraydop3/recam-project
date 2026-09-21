using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.DTOs.ListingCase;
using RecamNewBackend.Services;

namespace RecamNewBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ListingCaseController : ControllerBase
{
    private readonly IListingCaseService _service;

    public ListingCaseController(IListingCaseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Create a new listing case
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ListingCaseDto>>> Create([FromBody] CreateListingCaseDto dto)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.CreateAsync(dto, companyId);
        return Ok(ApiResponse<ListingCaseDto>.Success(result, "Listing case created successfully"));
    }

    /// <summary>
    /// Get all listing cases for the current company, with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ListingCaseDto>>>> GetAll([FromQuery] ListingCaseQueryDto query)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetAllAsync(companyId, query);
        return Ok(ApiResponse<PagedResult<ListingCaseDto>>.Success(result, "Listing cases retrieved successfully"));
    }

    /// <summary>
    /// Update a listing case
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ListingCaseDto>>> Update(int id, [FromBody] UpdateListingCaseDto dto)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.UpdateAsync(id, dto, companyId);
        return Ok(ApiResponse<ListingCaseDto>.Success(result, "Listing case updated successfully"));
    }

    /// <summary>
    /// Delete a listing case
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(id, companyId);
        return Ok(ApiResponse<string?>.Success(null, "Listing case deleted successfully"));
    }

    /// <summary>
    /// Get a listing case by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ListingCaseDto>>> GetById(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetByIdAsync(id, companyId);
        return Ok(ApiResponse<ListingCaseDto>.Success(result, "Listing case retrieved successfully"));
    }

    /// <summary>
    /// Change the status of a listing case
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<ListingCaseDto>>> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.ChangeStatusAsync(id, dto, userId);
        return Ok(ApiResponse<ListingCaseDto>.Success(result, "Status updated successfully"));
    }

    /// <summary>
    /// Assign (or unassign, with a null agentId) an agent to a listing case
    /// </summary>
    [HttpPatch("{id}/assign-agent")]
    public async Task<ActionResult<ApiResponse<ListingCaseDto>>> AssignAgent(int id, [FromBody] AssignAgentDto dto)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.AssignAgentAsync(id, dto, companyId);
        return Ok(ApiResponse<ListingCaseDto>.Success(result, "Agent assignment updated successfully"));
    }

    /// <summary>
    /// Generate a shareable public link token for a listing case
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<ActionResult<ApiResponse<string>>> GenerateShareToken(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var token = await _service.GenerateShareTokenAsync(id, companyId);
        return Ok(ApiResponse<string>.Success(token, "Share token generated successfully"));
    }

    /// <summary>
    /// View a listing case via shareable link — no login required
    /// </summary>
    [AllowAnonymous]
    [HttpGet("view/{token}")]
    public async Task<ActionResult<ApiResponse<ShareableLinkDto>>> ViewByToken(string token)
    {
        var result = await _service.GetByShareTokenAsync(token);
        return Ok(ApiResponse<ShareableLinkDto>.Success(result, "Listing case retrieved successfully"));
    }
}