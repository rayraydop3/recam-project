using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Common.Enums;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.Media;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Models;

namespace RecamNewBackend.Services;

public class MediaAssetService : IMediaAssetService
{
    private readonly RecamDbContext _context;
    private readonly IBlobService _blobService;

    private static readonly Dictionary<MediaType, HashSet<string>> AllowedExtensions = new()
    {
        { MediaType.Picture,   new() { ".jpg", ".jpeg", ".png", ".webp", ".heic" } },
        { MediaType.Video,     new() { ".mp4", ".mov", ".avi", ".mkv" } },
        { MediaType.FloorPlan, new() { ".jpg", ".jpeg", ".png", ".pdf" } }
    };

    public MediaAssetService(RecamDbContext context, IBlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    // Same reasoning as ListingCaseService.ResolveCompanyIdAsync — Agents
    // authenticate with their own user id, not their company's.
    private async Task<string> ResolveCompanyIdAsync(string userId)
    {
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == userId);
        return agent?.PhotographyCompanyId ?? userId;
    }

    public async Task<List<MediaAssetDto>> GetByListingCaseIdAsync(int listingCaseId, string companyId)
    {
        var resolvedCompanyId = await ResolveCompanyIdAsync(companyId);

        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == listingCaseId
                                   && c.PhotographyCompanyId == resolvedCompanyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {listingCaseId} not found.");

        var assets = await _context.MediaAssets
            .Where(m => m.ListingCaseId == listingCaseId && !m.IsDeleted)
            .ToListAsync();

        return assets.Select(MapToDto).ToList();
    }

    public async Task<List<MediaAssetDto>> UploadAsync(List<IFormFile> files, int listingCaseId, MediaType mediaType, string companyId)
    {
        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == listingCaseId
                                   && c.PhotographyCompanyId == companyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {listingCaseId} not found.");

        var allowed = AllowedExtensions[mediaType];
        var invalid = files
            .Where(f => !allowed.Contains(Path.GetExtension(f.FileName).ToLower()))
            .Select(f => f.FileName)
            .ToList();

        if (invalid.Any())
            throw new Exception($"Invalid file type(s): {string.Join(", ", invalid)}. Allowed: {string.Join(", ", allowed)}");

        var results = new List<MediaAsset>();

        foreach (var file in files)
        {
            var blobUrl = await _blobService.UploadAsync(file, "media");

            var asset = new MediaAsset
            {
                FileName = file.FileName,
                BlobUrl = blobUrl,
                MediaType = mediaType,
                ListingCaseId = listingCaseId
            };

            _context.MediaAssets.Add(asset);
            results.Add(asset);
        }

        await _context.SaveChangesAsync();

        return results.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(int id, string companyId)
    {
        var media = await _context.MediaAssets
            .Include(m => m.ListingCase)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (media == null)
            throw new NotFoundException($"Media asset {id} not found.");

        if (media.ListingCase.PhotographyCompanyId != companyId)
            throw new NotFoundException($"Media asset {id} not found.");

        media.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task SetCoverImageAsync(int mediaId, string companyId)
    {
        var media = await _context.MediaAssets
            .Include(m => m.ListingCase)
            .FirstOrDefaultAsync(m => m.Id == mediaId && !m.IsDeleted);

        if (media == null)
            throw new NotFoundException($"Media asset {mediaId} not found.");

        if (media.ListingCase.PhotographyCompanyId != companyId)
            throw new NotFoundException($"Media asset {mediaId} not found.");

        var allMedia = await _context.MediaAssets
            .Where(m => m.ListingCaseId == media.ListingCaseId && !m.IsDeleted)
            .ToListAsync();

        foreach (var m in allMedia)
            m.IsCoverImage = false;

        media.IsCoverImage = true;
        await _context.SaveChangesAsync();
    }

    public async Task<(Stream Content, string ContentType, string FileName)> DownloadAsync(int id, string companyId)
    {
        var resolvedCompanyId = await ResolveCompanyIdAsync(companyId);

        var media = await _context.MediaAssets
            .Include(m => m.ListingCase)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (media == null || media.ListingCase.PhotographyCompanyId != resolvedCompanyId)
            throw new NotFoundException($"Media asset {id} not found.");

        var stream = await _blobService.DownloadAsync(media.BlobUrl, "media");

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(media.FileName, out var contentType))
            contentType = "application/octet-stream";

        return (stream, contentType, media.FileName);
    }

    private static MediaAssetDto MapToDto(MediaAsset m) => new MediaAssetDto
    {
        Id = m.Id,
        FileName = m.FileName,
        BlobUrl = m.BlobUrl,
        MediaType = m.MediaType.ToString(),
        IsCoverImage = m.IsCoverImage,
        CreatedAt = m.CreatedAt
    };
}
