namespace Ladestander.Web.DTOs;

public record GenerateInvoiceRequestDto(
    int CustomerId,
    int BillingPeriodId
);