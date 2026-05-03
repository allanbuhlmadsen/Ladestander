namespace Ladestander.Api.Entities;

public class ChargingSession
{
    public int ChargingSessionId { get; set; }

    public int CustomerId { get; set; }
    public int BillingPeriodId { get; set; }
    public int? InvoiceId { get; set; }

    public string? ChargerAlias { get; set; }

    public DateTime? StartTime { get; set; }

    public decimal EnergyKWh { get; set; }

    public string? SourceUserName { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public BillingPeriod BillingPeriod { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}