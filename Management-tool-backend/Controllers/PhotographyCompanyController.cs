using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.DTOs.PhotographyCompany;
using RecamNewBackend.Services;

namespace RecamNewBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotographyCompanyController : ControllerBase
{
    private readonly IPhotographyCompanyService _service;

    public PhotographyCompanyController(IPhotographyCompanyService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all photography companies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PhotographyCompanyDto>>>> GetAll()
    {
        var companies = await _service.GetAllAsync();
        return Ok(ApiResponse<List<PhotographyCompanyDto>>.Success(companies, "Companies retrieved successfully"));
    }
}