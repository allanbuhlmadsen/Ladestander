namespace Ladestander.Api.Entities;
public class Customer
{
    public int CustomerId { get; set; }
    public int OldCustomerId { get; set; }

    public string RfidNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string? Email { get; set; }

    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public int? PostalCode { get; set; }
    public string? City { get; set; }

    public bool IsActive { get; set; }

    // Navigation
    public List<Invoice> Invoices { get; set; } = new();
    public List<ChargingSession> ChargingSessions { get; set; } = new();
}