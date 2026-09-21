using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using RecamNewBackend.DTOs.Auth;
using RecamNewBackend.Models;
using RecamNewBackend.Services;

namespace RecamNewBackend.Tests.Services;

public class AuthServiceTests
{
    private static Mock<UserManager<User>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IConfiguration CreateFakeConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "this-is-a-fake-test-signing-key-32chars" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpiresInDays", "7" }
        };

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        mockUserManager
            .Setup(m => m.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((User?)null);

        var service = new AuthService(mockUserManager.Object, CreateFakeConfiguration());
        var dto = new LoginDto { Email = "missing@example.com", Password = "Whatever1!" };

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        var user = new PhotographyCompany { Id = "user-1", Email = "test@example.com" };

        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

        var service = new AuthService(mockUserManager.Object, CreateFakeConfiguration());
        var dto = new LoginDto { Email = user.Email, Password = "WrongPassword" };

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        // Arrange
        var user = new PhotographyCompany { Id = "user-1", Email = "test@example.com" };

        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.CheckPasswordAsync(user, "Correct1!")).ReturnsAsync(true);
        mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var service = new AuthService(mockUserManager.Object, CreateFakeConfiguration());
        var dto = new LoginDto { Email = user.Email, Password = "Correct1!" };

        // Act
        var token = await service.LoginAsync(dto);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }
}
