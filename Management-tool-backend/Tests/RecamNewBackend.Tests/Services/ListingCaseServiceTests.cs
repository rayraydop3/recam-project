using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Common.Enums;
using RecamNewBackend.Data;
using RecamNewBackend.DTOs.ListingCase;
using RecamNewBackend.Services;

namespace RecamNewBackend.Tests.Services;

public class ListingCaseServiceTests
{
    private static RecamDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RecamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RecamDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveListingCase_WithCorrectFields()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new ListingCaseService(context);
        var dto = new CreateListingCaseDto
        {
            Address = "123 Test St",
            PropertyType = PropertyType.House,
            SaleType = SaleType.ForSale,
            Bedrooms = 3,
            Bathrooms = 2,
            Garages = 1,
            LandSize = 500
        };

        // Act
        var result = await service.CreateAsync(dto, "company-1");

        // Assert
        Assert.Equal("123 Test St", result.Address);
        Assert.Equal("Created", result.Status);
        Assert.Equal(1, await context.ListingCases.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAddress_AndOnlyReturnCurrentCompanysCases()
    {
        // Arrange
        await using var context = CreateContext();
        context.ListingCases.AddRange(
            new Models.ListingCase { Address = "1 Melbourne Rd", PhotographyCompanyId = "company-1" },
            new Models.ListingCase { Address = "2 Sydney Rd", PhotographyCompanyId = "company-1" },
            new Models.ListingCase { Address = "3 Melbourne St", PhotographyCompanyId = "company-1" },
            new Models.ListingCase { Address = "4 Melbourne Ave", PhotographyCompanyId = "company-2" }
        );
        await context.SaveChangesAsync();
        var service = new ListingCaseService(context);
        var query = new ListingCaseQueryDto { Address = "Melbourne", Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync("company-1", query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Contains("Melbourne", item.Address));
    }

    [Fact]
    public async Task GetAllAsync_ShouldRespectPageSize()
    {
        // Arrange
        await using var context = CreateContext();
        for (var i = 1; i <= 5; i++)
        {
            context.ListingCases.Add(new Models.ListingCase
            {
                Address = $"{i} Test St",
                PhotographyCompanyId = "company-1"
            });
        }
        await context.SaveChangesAsync();
        var service = new ListingCaseService(context);
        var query = new ListingCaseQueryDto { Page = 1, PageSize = 2 };

        // Act
        var result = await service.GetAllAsync("company-1", query);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalPages);
    }
}
