using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.CaseContact;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Models;

namespace RecamNewBackend.Services;

public class CaseContactService : ICaseContactService
{
    private readonly RecamDbContext _context;

    public CaseContactService(RecamDbContext context)
    {
        _context = context;
    }

    // Same reasoning as ListingCaseService.ResolveCompanyIdAsync — Agents
    // authenticate with their own user id, not their company's.
    private async Task<string> ResolveCompanyIdAsync(string userId)
    {
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == userId);
        return agent?.PhotographyCompanyId ?? userId;
    }

    public async Task<List<CaseContactDto>> GetByListingCaseIdAsync(int listingCaseId, string companyId)
    {
        var resolvedCompanyId = await ResolveCompanyIdAsync(companyId);

        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == listingCaseId
                                   && c.PhotographyCompanyId == resolvedCompanyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {listingCaseId} not found.");

        var contacts = await _context.CaseContacts
            .Where(c => c.ListingCaseId == listingCaseId && !c.IsDeleted)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<CaseContactDto> AddAsync(int listingCaseId, AddCaseContactDto dto, string companyId)
    {
        var resolvedCompanyId = await ResolveCompanyIdAsync(companyId);

        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == listingCaseId
                                   && c.PhotographyCompanyId == resolvedCompanyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {listingCaseId} not found.");

        var contact = new CaseContact
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyName = dto.CompanyName,
            ProfileImageUrl = dto.ProfileImageUrl,
            ListingCaseId = listingCaseId
        };

        _context.CaseContacts.Add(contact);
        await _context.SaveChangesAsync();

        return MapToDto(contact);
    }

    public async Task DeleteAsync(int id, string companyId)
    {
        var resolvedCompanyId = await ResolveCompanyIdAsync(companyId);

        var contact = await _context.CaseContacts
            .Include(c => c.ListingCase)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (contact == null)
            throw new NotFoundException($"Case contact {id} not found.");

        if (contact.ListingCase.PhotographyCompanyId != resolvedCompanyId)
            throw new NotFoundException($"Case contact {id} not found.");

        contact.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private static CaseContactDto MapToDto(CaseContact c) => new CaseContactDto
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        Phone = c.Phone,
        CompanyName = c.CompanyName,
        ProfileImageUrl = c.ProfileImageUrl,
        CreatedAt = c.CreatedAt
    };
}
