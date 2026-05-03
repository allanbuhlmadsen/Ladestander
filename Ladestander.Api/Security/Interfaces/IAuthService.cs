using Ladestander.Api.DTOs.Auth;

namespace Ladestander.Api.Security.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
}