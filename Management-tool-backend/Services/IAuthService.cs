using RecamNewBackend.DTOs.Auth;

namespace RecamNewBackend.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<CurrentUserDto> GetCurrentUserAsync(string userId);
    Task UpdatePasswordAsync(string userId, UpdatePasswordDto dto);
}