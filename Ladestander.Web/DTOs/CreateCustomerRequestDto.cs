namespace Ladestander.Web.DTOs;

public record CreateCustomerRequestDto(
    string RfidNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Email,
    string? Street,
    string? HouseNumber,
    int? PostalCode,
    string? City
);