using Ladestander.Api.Data;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Repositories
{
    public class BillingPeriodRepository : IBillingPeriodRepository
    {
        private readonly AppDbContext _context;

        public BillingPeriodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BillingPeriod>> GetAllAsync()
        {
            return await _context.BillingPeriods
                .ToListAsync();
        }

        public async Task<BillingPeriod?> GetByIdAsync(int billingPeriodId)
        {
            return await _context.BillingPeriods
                .FirstOrDefaultAsync(p => p.BillingPeriodId == billingPeriodId);
        }

        public async Task<BillingPeriod> AddAsync(BillingPeriod billingPeriod)
        {
            _context.BillingPeriods.Add(billingPeriod);
            await _context.SaveChangesAsync();
            return billingPeriod;
        }

        public async Task<BillingPeriod> UpdateAsync(BillingPeriod billingPeriod)
        {
            _context.BillingPeriods.Update(billingPeriod);
            await _context.SaveChangesAsync();
            return billingPeriod;
        }
    }
}