using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Moq;
using RecamNewBackend.Data;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Models;
using RecamNewBackend.Services;

namespace RecamNewBackend.Tests.Services;

public class SelectedMediaServiceTests
{
    private static RecamDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RecamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RecamDbContext(options);
    }

    [Fact]
    public async Task DownloadZipAsync_ShouldThrowNotFound_WhenListingCaseBelongsToAnotherCompany()
    {
        // Arrange
        await using var context = CreateContext();
        context.ListingCases.Add(new Models.ListingCase { Id = 1, Address = "Test", PhotographyCompanyId = "company-1" });
        await context.SaveChangesAsync();

        var mockBlobService = new Mock<IBlobService>();
        var service = new SelectedMediaService(context, mockBlobService.Object);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DownloadZipAsync(1, "company-2"));
    }

    [Fact]
    public async Task DownloadZipAsync_ShouldThrowNotFound_WhenNoMediaIsSelected()
    {
        // Arrange
        await using var context = CreateContext();
        context.ListingCases.Add(new Models.ListingCase { Id = 1, Address = "Test", PhotographyCompanyId = "company-1" });
        await context.SaveChangesAsync();

        var mockBlobService = new Mock<IBlobService>();
        var service = new SelectedMediaService(context, mockBlobService.Object);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DownloadZipAsync(1, "company-1"));
    }

    [Fact]
    public async Task DownloadZipAsync_ShouldReturnZip_ContainingOnlySelectedMedia()
    {
        // Arrange
        await using var context = CreateContext();
        var listingCase = new Models.ListingCase { Id = 1, Address = "123 Test St", PhotographyCompanyId = "company-1" };
        var selectedMedia = new MediaAsset { Id = 1, FileName = "selected.jpg", BlobUrl = "https://fake/media/selected.jpg", ListingCaseId = 1 };
        var unselectedMedia = new MediaAsset { Id = 2, FileName = "unselected.jpg", BlobUrl = "https://fake/media/unselected.jpg", ListingCaseId = 1 };

        context.ListingCases.Add(listingCase);
        context.MediaAssets.AddRange(selectedMedia, unselectedMedia);
        context.SelectedMedias.Add(new SelectedMedia { ListingCaseId = 1, MediaAssetId = 1, AgentId = "agent-1", IsSelected = true });
        context.SelectedMedias.Add(new SelectedMedia { ListingCaseId = 1, MediaAssetId = 2, AgentId = "agent-1", IsSelected = false });
        await context.SaveChangesAsync();

        var fakeContent = Encoding.UTF8.GetBytes("fake image bytes");
        var mockBlobService = new Mock<IBlobService>();
        mockBlobService
            .Setup(b => b.DownloadAsync(selectedMedia.BlobUrl, "media"))
            .ReturnsAsync(() => new MemoryStream(fakeContent));

        var service = new SelectedMediaService(context, mockBlobService.Object);

        // Act
        var (zipBytes, _) = await service.DownloadZipAsync(1, "company-1");

        // Assert
        using var zipStream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        Assert.Single(archive.Entries);
        Assert.Equal($"{selectedMedia.Id}_{selectedMedia.FileName}", archive.Entries[0].Name);

        mockBlobService.Verify(b => b.DownloadAsync(selectedMedia.BlobUrl, "media"), Times.Once);
        mockBlobService.Verify(b => b.DownloadAsync(unselectedMedia.BlobUrl, "media"), Times.Never);
    }
}
