using RecamNewBackend.Common;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.CaseContact;
using RecamNewBackend.DTOs.ListingCase;
using RecamNewBackend.DTOs.Media;
using RecamNewBackend.Models;
using RecamNewBackend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace RecamNewBackend.Services;

public class ListingCaseService : IListingCaseService
{
    private readonly RecamDbContext _context;

    public ListingCaseService(RecamDbContext context)
    {
        _context = context;
    }

    // Listing cases are scoped by PhotographyCompanyId, but Agents authenticate with
    // their own user id, not their company's — resolve it here so both roles can read.
    // Returning the Agent itself (not just the resolved id) lets callers that need it
    // also restrict results to cases assigned to that specific Agent.
    private Task<Agent?> FindAgentAsync(string userId) =>
        _context.Agents.FirstOrDefaultAsync(a => a.Id == userId);

    public async Task<ListingCaseDto> CreateAsync(CreateListingCaseDto dto, string companyId)
    {
        var listingCase = new ListingCase
        {
            Address = dto.Address,
            PropertyType = dto.PropertyType,
            SaleType = dto.SaleType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            Garages = dto.Garages,
            LandSize = dto.LandSize,
            PhotographyCompanyId = companyId
        };

        _context.ListingCases.Add(listingCase);
        await _context.SaveChangesAsync();

        return new ListingCaseDto
        {
            Id = listingCase.Id,
            Address = listingCase.Address,
            Status = listingCase.Status.ToString(),
            PropertyType = listingCase.PropertyType.ToString(),
            SaleType = listingCase.SaleType.ToString(),
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            LandSize = listingCase.LandSize,
            CreatedAt = listingCase.CreatedAt,
            PhotographyCompanyId = listingCase.PhotographyCompanyId,
            AgentId = listingCase.AgentId
        };
    }

    public async Task<PagedResult<ListingCaseDto>> GetAllAsync(string companyId, ListingCaseQueryDto query)
    {
        var agent = await FindAgentAsync(companyId);
        var resolvedCompanyId = agent?.PhotographyCompanyId ?? companyId;

        var listingCases = _context.ListingCases
            .Where(c => c.PhotographyCompanyId == resolvedCompanyId && !c.IsDeleted);

        // Agents only see cases assigned to them; the company itself sees everything.
        if (agent != null)
            listingCases = listingCases.Where(c => c.AgentId == companyId);

        if (query.Status.HasValue)
            listingCases = listingCases.Where(c => c.Status == query.Status.Value);

        if (query.PropertyType.HasValue)
            listingCases = listingCases.Where(c => c.PropertyType == query.PropertyType.Value);

        if (query.SaleType.HasValue)
            listingCases = listingCases.Where(c => c.SaleType == query.SaleType.Value);

        if (!string.IsNullOrWhiteSpace(query.Address))
            listingCases = listingCases.Where(c => c.Address.Contains(query.Address));

        var totalCount = await listingCases.CountAsync();

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var items = await listingCases
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ListingCaseDto
            {
                Id = c.Id,
                Address = c.Address,
                Status = c.Status.ToString(),
                PropertyType = c.PropertyType.ToString(),
                SaleType = c.SaleType.ToString(),
                Bedrooms = c.Bedrooms,
                Bathrooms = c.Bathrooms,
                Garages = c.Garages,
                LandSize = c.LandSize,
                CreatedAt = c.CreatedAt,
                PhotographyCompanyId = c.PhotographyCompanyId,
                AgentId = c.AgentId
            })
            .ToListAsync();

        return new PagedResult<ListingCaseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ListingCaseDto> UpdateAsync(int id, UpdateListingCaseDto dto, string companyId)
    {
        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == id
                                    && c.PhotographyCompanyId == companyId
                                    && !c.IsDeleted);

        if (listingCase == null)
        {
            throw new NotFoundException($"Listing case {id} not found.");
        }

        listingCase.Address = dto.Address;
        listingCase.PropertyType = dto.PropertyType;
        listingCase.SaleType = dto.SaleType;
        listingCase.Bedrooms = dto.Bedrooms;
        listingCase.Bathrooms = dto.Bathrooms;
        listingCase.Garages = dto.Garages;
        listingCase.LandSize = dto.LandSize;

        await _context.SaveChangesAsync();

        return new ListingCaseDto
        {
            Id = listingCase.Id,
            Address = listingCase.Address,
            Status = listingCase.Status.ToString(),
            PropertyType = listingCase.PropertyType.ToString(),
            SaleType = listingCase.SaleType.ToString(),
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            LandSize = listingCase.LandSize,
            CreatedAt = listingCase.CreatedAt,
            PhotographyCompanyId = listingCase.PhotographyCompanyId,
            AgentId = listingCase.AgentId
        };
    }

    public async Task DeleteAsync(int id, string companyId)
    {
        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == id
                                    && c.PhotographyCompanyId == companyId
                                    && !c.IsDeleted);

        if (listingCase == null)
        {
            throw new NotFoundException($"Listing case {id} not found.");
        }

        listingCase.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<ListingCaseDto> GetByIdAsync(int id, string companyId)
    {
        var agent = await FindAgentAsync(companyId);
        var resolvedCompanyId = agent?.PhotographyCompanyId ?? companyId;

        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == id
                                    && c.PhotographyCompanyId == resolvedCompanyId
                                    && (agent == null || c.AgentId == companyId)
                                    && !c.IsDeleted);

        if (listingCase == null)
        {
            throw new NotFoundException($"Listing case {id} not found.");
        }

        return new ListingCaseDto
        {
            Id = listingCase.Id,
            Address = listingCase.Address,
            Status = listingCase.Status.ToString(),
            PropertyType = listingCase.PropertyType.ToString(),
            SaleType = listingCase.SaleType.ToString(),
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            LandSize = listingCase.LandSize,
            CreatedAt = listingCase.CreatedAt,
            PhotographyCompanyId = listingCase.PhotographyCompanyId,
            AgentId = listingCase.AgentId
        };
    }

    public async Task<ListingCaseDto> ChangeStatusAsync(int id, ChangeStatusDto dto, string userId)
    {
        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == id
                                    && c.PhotographyCompanyId == userId
                                    && !c.IsDeleted);

        if (listingCase == null)
        {
            throw new NotFoundException($"Listing case {id} not found.");
        }

        var oldStatus = listingCase.Status;

        var history = new StatusHistory
        {
            OldStatus = oldStatus,
            NewStatus = dto.Status,
            ListingCaseId = listingCase.Id,
            ChangedByUserId = userId
        };

        listingCase.Status = dto.Status;

        _context.StatusHistories.Add(history);
        await _context.SaveChangesAsync();

        return new ListingCaseDto
        {
            Id = listingCase.Id,
            Address = listingCase.Address,
            Status = listingCase.Status.ToString(),
            PropertyType = listingCase.PropertyType.ToString(),
            SaleType = listingCase.SaleType.ToString(),
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            LandSize = listingCase.LandSize,
            CreatedAt = listingCase.CreatedAt,
            PhotographyCompanyId = listingCase.PhotographyCompanyId,
            AgentId = listingCase.AgentId
        };
    }

    public async Task<string> GenerateShareTokenAsync(int id, string companyId)
    {
        var agent = await FindAgentAsync(companyId);
        var resolvedCompanyId = agent?.PhotographyCompanyId ?? companyId;

        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == id
                                   && c.PhotographyCompanyId == resolvedCompanyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {id} not found.");

        listingCase.ShareToken = Guid.NewGuid().ToString("N");
        await _context.SaveChangesAsync();

        return listingCase.ShareToken;
    }

    public async Task<ListingCaseDto> AssignAgentAsync(int id, AssignAgentDto dto, string companyId)
    {
        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == id
                                   && c.PhotographyCompanyId == companyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {id} not found.");

        if (dto.AgentId != null)
        {
            var agentBelongsToCompany = await _context.Agents
                .AnyAsync(a => a.Id == dto.AgentId && a.PhotographyCompanyId == companyId);

            if (!agentBelongsToCompany)
                throw new NotFoundException($"Agent {dto.AgentId} not found.");
        }

        listingCase.AgentId = dto.AgentId;
        await _context.SaveChangesAsync();

        return new ListingCaseDto
        {
            Id = listingCase.Id,
            Address = listingCase.Address,
            Status = listingCase.Status.ToString(),
            PropertyType = listingCase.PropertyType.ToString(),
            SaleType = listingCase.SaleType.ToString(),
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            LandSize = listingCase.LandSize,
            CreatedAt = listingCase.CreatedAt,
            PhotographyCompanyId = listingCase.PhotographyCompanyId,
            AgentId = listingCase.AgentId
        };
    }

    public async Task<ShareableLinkDto> GetByShareTokenAsync(string token)
    {
        var listingCase = await _context.ListingCases
            .Include(c => c.MediaAssets.Where(m => !m.IsDeleted))
            .Include(c => c.CaseContacts.Where(cc => !cc.IsDeleted))
            .FirstOrDefaultAsync(c => c.ShareToken == token && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException("Invalid or expired share link.");

        return new ShareableLinkDto
        {
            Id = listingCase.Id,
            Address = listingCase.Address,
            Status = listingCase.Status.ToString(),
            PropertyType = listingCase.PropertyType.ToString(),
            SaleType = listingCase.SaleType.ToString(),
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            LandSize = listingCase.LandSize,
            MediaAssets = listingCase.MediaAssets.Select(m => new MediaAssetDto
            {
                Id = m.Id,
                FileName = m.FileName,
                BlobUrl = m.BlobUrl,
                MediaType = m.MediaType.ToString(),
                IsCoverImage = m.IsCoverImage,
                CreatedAt = m.CreatedAt
            }).ToList(),
            CaseContacts = listingCase.CaseContacts.Select(cc => new CaseContactDto
            {
                Id = cc.Id,
                FirstName = cc.FirstName,
                LastName = cc.LastName,
                Email = cc.Email,
                Phone = cc.Phone,
                CompanyName = cc.CompanyName,
                ProfileImageUrl = cc.ProfileImageUrl,
                CreatedAt = cc.CreatedAt
            }).ToList()
        };
    }
}