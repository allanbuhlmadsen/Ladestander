using System.ComponentModel.DataAnnotations;

namespace Ladestander.Api.DTOs.Invoices
{
    public record GenerateInvoiceRequestDto(
        [Range(1, int.MaxValue)]
        int CustomerId,

        [Range(1, int.MaxValue)]
        int BillingPeriodId
    );
}