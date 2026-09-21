using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.DTOs.CaseContact;
using RecamNewBackend.Services;
using System.Security.Claims;

namespace RecamNewBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CaseContactController : ControllerBase
{
    private readonly ICaseContactService _service;

    public CaseContactController(ICaseContactService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all contacts for a listing case
    /// </summary>
    [HttpGet("{listingCaseId}")]
    public async Task<ActionResult<ApiResponse<List<CaseContactDto>>>> GetByListingCase(int listingCaseId)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetByListingCaseIdAsync(listingCaseId, companyId);
        return Ok(ApiResponse<List<CaseContactDto>>.Success(result, "Contacts retrieved successfully"));
    }

    /// <summary>
    /// Add a contact to a listing case
    /// </summary>
    [HttpPost("{listingCaseId}")]
    public async Task<ActionResult<ApiResponse<CaseContactDto>>> Add(int listingCaseId, [FromBody] AddCaseContactDto dto)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.AddAsync(listingCaseId, dto, companyId);
        return Ok(ApiResponse<CaseContactDto>.Success(result, "Contact added successfully"));
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(id, companyId);
        return Ok(ApiResponse<string>.Success("", "Contact deleted successfully"));
    }
}
