using Ladestander.Api.Entities;

namespace Ladestander.Api.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(int invoiceId);
        Task<List<Invoice>> GetAllAsync();
        Task<Invoice> AddAsync(Invoice invoice);
        Task<bool> ExistsAsync(int customerId, int billingPeriodId);
        Task<Invoice> UpdateAsync(Invoice invoice);
        Task<Invoice?> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId);
    }
}