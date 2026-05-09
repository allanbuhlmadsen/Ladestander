namespace Ladestander.Web.DTOs;

public record CustomerResponseDto(
    int CustomerId,
    string RfidNumber,
    string FullName,
    string? Email,
    string? Street,
    string? HouseNumber,
    int? PostalCode,
    string? City,
    bool IsActive
);