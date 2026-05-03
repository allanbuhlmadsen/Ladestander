using System.ComponentModel.DataAnnotations;

namespace Ladestander.Api.DTOs.Customers;

public record CreateCustomerRequestDto(
    [Required]
    [MaxLength(20)]
    string RfidNumber,

    [Required]
    [MaxLength(255)]
    string FirstName,

    [MaxLength(255)]
    string? MiddleName,

    [Required]
    [MaxLength(255)]
    string LastName,

    [EmailAddress]
    [MaxLength(255)]
    string? Email,

    [MaxLength(255)]
    string? Street,

    [MaxLength(20)]
    string? HouseNumber,

    int? PostalCode,

    [MaxLength(255)]
    string? City
);