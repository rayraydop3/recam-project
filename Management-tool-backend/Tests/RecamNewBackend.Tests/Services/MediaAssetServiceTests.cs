using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RecamNewBackend.Common.Enums;
using RecamNewBackend.Data;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Services;

namespace RecamNewBackend.Tests.Services;

public class MediaAssetServiceTests
{
    private static RecamDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RecamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RecamDbContext(options);
    }

    private static Mock<IFormFile> CreateFakeFile(string fileName)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        return mockFile;
    }

    [Fact]
    public async Task UploadAsync_ShouldThrow_WhenExtensionNotAllowedForMediaType()
    {
        // Arrange
        await using var context = CreateContext();
        context.ListingCases.Add(new Models.ListingCase { Id = 1, Address = "Test", PhotographyCompanyId = "company-1" });
        await context.SaveChangesAsync();

        var mockFile = CreateFakeFile("notes.txt");
        var mockBlobService = new Mock<IBlobService>();
        var service = new MediaAssetService(context, mockBlobService.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => service.UploadAsync(new List<IFormFile> { mockFile.Object }, 1, MediaType.Picture, "company-1"));

        Assert.Contains("Invalid file type", ex.Message);
        mockBlobService.Verify(b => b.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowNotFound_WhenListingCaseBelongsToAnotherCompany()
    {
        // Arrange
        await using var context = CreateContext();
        context.ListingCases.Add(new Models.ListingCase { Id = 1, Address = "Test", PhotographyCompanyId = "company-1" });
        await context.SaveChangesAsync();

        var mockFile = CreateFakeFile("photo.jpg");
        var mockBlobService = new Mock<IBlobService>();
        var service = new MediaAssetService(context, mockBlobService.Object);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UploadAsync(new List<IFormFile> { mockFile.Object }, 1, MediaType.Picture, "company-2"));
    }

    [Fact]
    public async Task UploadAsync_ShouldSaveMediaAsset_WhenFileIsValid()
    {
        // Arrange
        await using var context = CreateContext();
        context.ListingCases.Add(new Models.ListingCase { Id = 1, Address = "Test", PhotographyCompanyId = "company-1" });
        await context.SaveChangesAsync();

        var mockFile = CreateFakeFile("photo.jpg");
        var mockBlobService = new Mock<IBlobService>();
        mockBlobService
            .Setup(b => b.UploadAsync(mockFile.Object, "media"))
            .ReturnsAsync("https://fake/media/photo.jpg");

        var service = new MediaAssetService(context, mockBlobService.Object);

        // Act
        var result = await service.UploadAsync(new List<IFormFile> { mockFile.Object }, 1, MediaType.Picture, "company-1");

        // Assert
        Assert.Single(result);
        Assert.Equal("https://fake/media/photo.jpg", result[0].BlobUrl);
        Assert.Equal(1, await context.MediaAssets.CountAsync());
    }
}
