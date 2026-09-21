using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.DTOs.SelectedMedia;
using RecamNewBackend.Services;
using System.Security.Claims;

namespace RecamNewBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SelectedMediaController : ControllerBase
{
    private readonly ISelectedMediaService _service;

    public SelectedMediaController(ISelectedMediaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Select or deselect a media asset for a listing case
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<SelectedMediaResultDto>>> Select([FromBody] SelectMediaDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.SelectAsync(dto, userId);
        return Ok(ApiResponse<SelectedMediaResultDto>.Success(result, "Media selection updated"));
    }

    /// <summary>
    /// Get all selected media for a listing case
    /// </summary>
    [HttpGet("{listingCaseId}/final")]
    public async Task<ActionResult<ApiResponse<List<SelectedMediaResultDto>>>> GetFinalSelection(int listingCaseId)
    {
        var result = await _service.GetFinalSelectionAsync(listingCaseId);
        return Ok(ApiResponse<List<SelectedMediaResultDto>>.Success(result, "Final selection retrieved successfully"));
    }

    /// <summary>
    /// Get the full media selection history (every select/deselect action) for a listing case
    /// </summary>
    [HttpGet("{listingCaseId}/history")]
    public async Task<ActionResult<ApiResponse<List<MediaSelectionHistoryDto>>>> GetHistory(int listingCaseId)
    {
        var result = await _service.GetHistoryAsync(listingCaseId);
        return Ok(ApiResponse<List<MediaSelectionHistoryDto>>.Success(result, "Selection history retrieved successfully"));
    }

    /// <summary>
    /// Download all agent-selected media for a listing case as a ZIP file
    /// </summary>
    [HttpGet("{listingCaseId}/download-zip")]
    public async Task<IActionResult> DownloadZip(int listingCaseId)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (content, fileName) = await _service.DownloadZipAsync(listingCaseId, companyId);
        return File(content, "application/zip", fileName);
    }
}
