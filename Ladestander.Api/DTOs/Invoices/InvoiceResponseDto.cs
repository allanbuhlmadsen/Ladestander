namespace Ladestander.Api.DTOs.Invoices
{
    public record InvoiceResponseDto(
        int InvoiceId,
        int OldInvoiceId,
        int CustomerId,
        int BillingPeriodId,
        string? InvoiceNumber,
        bool IsPaid,
        bool IsSent,
        string? Status,
        decimal TotalEnergyKWh,
        decimal TotalAmount,
        DateTime? CreatedAt
    );
}