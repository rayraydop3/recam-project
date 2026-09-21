using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RecamNewBackend.DTOs.Auth;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Models;


namespace RecamNewBackend.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
{
    var company = new PhotographyCompany
    {
        UserName = dto.Email,
        Email = dto.Email,
        PhoneNumber = dto.Phone,   // IdentityUser 用的是 PhoneNumber
        Name = dto.CompanyName,
    };

    var result = await _userManager.CreateAsync(company, dto.Password);

    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new Exception(errors);
    }

    await _userManager.AddToRoleAsync(company, "Admin");

    return GenerateJwtToken(company, new List<string> { "Admin" });
}

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || user.IsDeleted)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);

        if (!isPasswordCorrect)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return GenerateJwtToken(user, roles);
    }

    public async Task UpdatePasswordAsync(string userId, UpdatePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception(errors);
        }
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);

    if (user == null || user.IsDeleted)
        throw new NotFoundException("User not found.");

    var company = user as PhotographyCompany;

    return new CurrentUserDto
    {
        Id = user.Id,
        Name = company?.Name ?? string.Empty,
        Email = user.Email!,
        Phone = user.PhoneNumber,
        CreatedAt = user.CreatedAt
    };
}

    private string GenerateJwtToken(User user, IList<string> roles)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email!)
    };

    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:ExpiresInDays"]!)),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}