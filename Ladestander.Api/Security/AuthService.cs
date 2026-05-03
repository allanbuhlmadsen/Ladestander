using Ladestander.Api.DTOs.Auth;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Security.Interfaces;

namespace Ladestander.Api.Security;

public class AuthService : IAuthService
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var adminUser = await _adminUserRepository.GetByUsernameAsync(username);

        if (adminUser is null)
        {
            return null;
        }

        bool passwordIsValid = _passwordHasher.Verify(password, adminUser.PasswordHash);

        if (!passwordIsValid)
        {
            return null;
        }

        string token = _jwtService.GenerateToken(adminUser);

        return new LoginResponseDto
        {
            Token = token,
            Username = adminUser.Username,
            Role = adminUser.Role
        };
    }
}