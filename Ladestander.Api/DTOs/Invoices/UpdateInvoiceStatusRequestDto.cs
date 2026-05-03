using System.ComponentModel.DataAnnotations;

namespace Ladestander.Api.DTOs.Invoices
{
    public record UpdateInvoiceStatusRequestDto(
        [Required]
        [MaxLength(50)]
        string Status
    );
}