namespace Ladestander.Api.DTOs.Customers;

public record CustomerResponseDto(
    int CustomerId,
    string RfidNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    string FullName,
    string? Email,
    string? Street,
    string? HouseNumber,
    int? PostalCode,
    string? City,
    bool IsActive
);