using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.Common.Enums;
using RecamNewBackend.DTOs.Media;
using RecamNewBackend.Services;
using System.Security.Claims;

namespace RecamNewBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaAssetService _service;

    public MediaController(IMediaAssetService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all media assets for a listing case
    /// </summary>
    [HttpGet("{listingCaseId}")]
    public async Task<ActionResult<ApiResponse<List<MediaAssetDto>>>> GetByListingCase(int listingCaseId)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetByListingCaseIdAsync(listingCaseId, companyId);
        return Ok(ApiResponse<List<MediaAssetDto>>.Success(result, "Media assets retrieved successfully"));
    }

    /// <summary>
    /// Upload media files to a listing case (one media type at a time)
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<List<MediaAssetDto>>>> Upload(
        [FromForm] List<IFormFile> files,
        [FromForm] int listingCaseId,
        [FromForm] MediaType mediaType)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.UploadAsync(files, listingCaseId, mediaType, companyId);
        return Ok(ApiResponse<List<MediaAssetDto>>.Success(result, "Files uploaded successfully"));
    }

    /// <summary>
    /// Download a single media asset with its original filename
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (content, contentType, fileName) = await _service.DownloadAsync(id, companyId);
        return File(content, contentType, fileName);
    }

    /// <summary>
    /// Set a media asset as the cover image for its listing case
    /// </summary>
    [HttpPatch("{id}/cover")]
    public async Task<ActionResult<ApiResponse<string>>> SetCoverImage(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.SetCoverImageAsync(id, companyId);
        return Ok(ApiResponse<string>.Success("", "Cover image set successfully"));
    }

    /// <summary>
    /// Soft delete a media asset
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(id, companyId);
        return Ok(ApiResponse<string>.Success("", "Media asset deleted successfully"));
    }
}
