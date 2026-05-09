namespace Ladestander.Web.DTOs.Auth;

public record LoginResponseDto(
    string Token,
    string Username,
    string Role
);