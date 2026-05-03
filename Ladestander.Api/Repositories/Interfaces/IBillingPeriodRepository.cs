using Ladestander.Api.Entities;

namespace Ladestander.Api.Repositories.Interfaces
{
    public interface IBillingPeriodRepository
    {
        Task<List<BillingPeriod>> GetAllAsync();
        Task<BillingPeriod?> GetByIdAsync(int billingPeriodId);
        Task<BillingPeriod> AddAsync(BillingPeriod billingPeriod);
        Task<BillingPeriod> UpdateAsync(BillingPeriod billingPeriod);
    }
}