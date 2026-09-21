using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.PhotographyCompany;

namespace RecamNewBackend.Services;

public class PhotographyCompanyService : IPhotographyCompanyService
{
    private readonly RecamDbContext _context;

    public PhotographyCompanyService(RecamDbContext context)
    {
        _context = context;
    }

    public async Task<List<PhotographyCompanyDto>> GetAllAsync()
    {
        return await _context.PhotographyCompanies
            .Where(c => !c.IsDeleted)
            .Select(c => new PhotographyCompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email ?? string.Empty,
                Phone = c.PhoneNumber,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }
}