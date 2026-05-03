using Ladestander.Api.Data;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(int invoiceId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .OrderBy(i => i.InvoiceId)
                .ToListAsync();
        }

        public async Task<Invoice> AddAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<bool> ExistsAsync(int customerId, int billingPeriodId)
        {
            return await _context.Invoices
                .AnyAsync(i =>
                    i.CustomerId == customerId &&
                    i.BillingPeriodId == billingPeriodId);
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<Invoice?> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i =>
                    i.CustomerId == customerId &&
                    i.BillingPeriodId == billingPeriodId);
        }
    }
}