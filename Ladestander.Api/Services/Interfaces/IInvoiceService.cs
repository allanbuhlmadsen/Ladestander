using Ladestander.Api.DTOs.Invoices;

namespace Ladestander.Api.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<List<InvoiceResponseDto>> GetAllAsync();
        Task<InvoiceResponseDto?> GetByIdAsync(int invoiceId);
        Task<InvoiceResponseDto> GenerateAsync(GenerateInvoiceRequestDto request);
        Task<InvoiceResponseDto?> UpdateStatusAsync(int invoiceId, UpdateInvoiceStatusRequestDto request);
        Task<InvoiceResponseDto?> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId);
    }
}