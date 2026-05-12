namespace Ladestander.Web.DTOs;

public record ErrorResponseDto(
    int StatusCode,
    string Message,
    DateTime Timestamp,
    Dictionary<string, string[]>? Errors
);