using RecamNewBackend.Common.Enums;
using RecamNewBackend.DTOs.Media;

namespace RecamNewBackend.Services;

public interface IMediaAssetService
{
    Task<List<MediaAssetDto>> GetByListingCaseIdAsync(int listingCaseId, string companyId);
    Task DeleteAsync(int id, string companyId);
    Task SetCoverImageAsync(int mediaId, string companyId);
    Task<List<MediaAssetDto>> UploadAsync(List<IFormFile> files, int listingCaseId, MediaType mediaType, string companyId);
    Task<(Stream Content, string ContentType, string FileName)> DownloadAsync(int id, string companyId);
}
