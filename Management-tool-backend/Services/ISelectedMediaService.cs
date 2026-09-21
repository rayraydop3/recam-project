using RecamNewBackend.DTOs.SelectedMedia;

namespace RecamNewBackend.Services;

public interface ISelectedMediaService
{
    Task<SelectedMediaResultDto> SelectAsync(SelectMediaDto dto, string userId);
    Task<List<SelectedMediaResultDto>> GetFinalSelectionAsync(int listingCaseId);
    Task<List<MediaSelectionHistoryDto>> GetHistoryAsync(int listingCaseId);
    Task<(byte[] Content, string FileName)> DownloadZipAsync(int listingCaseId, string companyId);
}
