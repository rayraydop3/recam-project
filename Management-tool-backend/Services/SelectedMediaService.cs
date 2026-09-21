using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.SelectedMedia;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Models;
using System.IO.Compression;

namespace RecamNewBackend.Services;

public class SelectedMediaService : ISelectedMediaService
{
    private readonly RecamDbContext _context;
    private readonly IBlobService _blobService;

    public SelectedMediaService(RecamDbContext context, IBlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    public async Task<SelectedMediaResultDto> SelectAsync(SelectMediaDto dto, string userId)
    {
        var existing = await _context.SelectedMedias
            .Include(sm => sm.MediaAsset)
            .FirstOrDefaultAsync(sm => sm.ListingCaseId == dto.ListingCaseId
                                    && sm.MediaAssetId == dto.MediaAssetId
                                    && sm.AgentId == userId);

        if (existing != null)
        {
            existing.IsSelected = dto.IsSelected;
            existing.SelectedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new SelectedMedia
            {
                ListingCaseId = dto.ListingCaseId,
                MediaAssetId = dto.MediaAssetId,
                AgentId = userId,
                IsSelected = dto.IsSelected,
                SelectedAt = DateTime.UtcNow
            };
            _context.SelectedMedias.Add(existing);
        }

        _context.MediaSelectionHistories.Add(new MediaSelectionHistory
        {
            ListingCaseId = dto.ListingCaseId,
            MediaAssetId = dto.MediaAssetId,
            AgentId = userId,
            IsSelected = dto.IsSelected
        });

        await _context.SaveChangesAsync();
        await _context.Entry(existing).Reference(sm => sm.MediaAsset).LoadAsync();

        return MapToDto(existing);
    }

    public async Task<List<SelectedMediaResultDto>> GetFinalSelectionAsync(int listingCaseId)
    {
        var selections = await _context.SelectedMedias
            .Include(sm => sm.MediaAsset)
            .Where(sm => sm.ListingCaseId == listingCaseId && sm.IsSelected)
            .ToListAsync();

        return selections.Select(MapToDto).ToList();
    }

    public async Task<List<MediaSelectionHistoryDto>> GetHistoryAsync(int listingCaseId)
    {
        var history = await _context.MediaSelectionHistories
            .Include(h => h.MediaAsset)
            .Include(h => h.Agent)
            .Where(h => h.ListingCaseId == listingCaseId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        return history.Select(h => new MediaSelectionHistoryDto
        {
            Id = h.Id,
            MediaAssetId = h.MediaAssetId,
            FileName = h.MediaAsset.FileName,
            IsSelected = h.IsSelected,
            ChangedAt = h.ChangedAt,
            AgentId = h.AgentId,
            AgentName = $"{h.Agent.FirstName} {h.Agent.LastName}"
        }).ToList();
    }

    public async Task<(byte[] Content, string FileName)> DownloadZipAsync(int listingCaseId, string companyId)
    {
        var listingCase = await _context.ListingCases
            .FirstOrDefaultAsync(c => c.Id == listingCaseId
                                   && c.PhotographyCompanyId == companyId
                                   && !c.IsDeleted);

        if (listingCase == null)
            throw new NotFoundException($"Listing case {listingCaseId} not found.");

        var selections = await _context.SelectedMedias
            .Include(sm => sm.MediaAsset)
            .Where(sm => sm.ListingCaseId == listingCaseId && sm.IsSelected)
            .ToListAsync();

        if (!selections.Any())
            throw new NotFoundException("No selected media found for this listing case.");

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var sm in selections)
            {
                var entryName = $"{sm.MediaAsset.Id}_{sm.MediaAsset.FileName}";
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);

                using var entryStream = entry.Open();
                using var blobStream = await _blobService.DownloadAsync(sm.MediaAsset.BlobUrl, "media");
                await blobStream.CopyToAsync(entryStream);
            }
        }

        var safeAddress = string.Concat(listingCase.Address.Split(Path.GetInvalidFileNameChars()));
        return (zipStream.ToArray(), $"{safeAddress}-selected-media.zip");
    }

    private static SelectedMediaResultDto MapToDto(SelectedMedia sm) => new SelectedMediaResultDto
    {
        Id = sm.Id,
        MediaAssetId = sm.MediaAssetId,
        FileName = sm.MediaAsset.FileName,
        BlobUrl = sm.MediaAsset.BlobUrl,
        MediaType = sm.MediaAsset.MediaType.ToString(),
        IsCoverImage = sm.MediaAsset.IsCoverImage,
        IsSelected = sm.IsSelected,
        SelectedAt = sm.SelectedAt
    };
}
