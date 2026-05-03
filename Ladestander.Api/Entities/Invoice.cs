namespace Ladestander.Api.Entities;

public class Invoice
{
    public int InvoiceId { get; set; }
    public int OldInvoiceId { get; set; }

    public int CustomerId { get; set; }
    public int BillingPeriodId { get; set; }

    public string? InvoiceNumber { get; set; }

    public bool IsPaid { get; set; }
    public bool IsSent { get; set; }

    public string? Status { get; set; }

    public decimal TotalEnergyKWh { get; set; }
    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public BillingPeriod BillingPeriod { get; set; } = null!;
    public List<ChargingSession> ChargingSessions { get; set; } = new();
}