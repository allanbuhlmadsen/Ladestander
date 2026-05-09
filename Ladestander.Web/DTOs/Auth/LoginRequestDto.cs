namespace Ladestander.Web.DTOs.Auth;

public record LoginRequestDto(
    string Username,
    string Password
);