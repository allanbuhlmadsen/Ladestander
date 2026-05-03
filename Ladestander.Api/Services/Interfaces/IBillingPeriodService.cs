using Ladestander.Api.DTOs.BillingPeriods;

namespace Ladestander.Api.Services.Interfaces
{
    public interface IBillingPeriodService
    {
        Task<List<BillingPeriodResponseDto>> GetAllAsync();
        Task<BillingPeriodResponseDto?> GetByIdAsync(int billingPeriodId);
        Task<BillingPeriodResponseDto> CreateAsync(CreateBillingPeriodRequestDto request);
        Task<BillingPeriodResponseDto?> UpdateAsync(int billingPeriodId, UpdateBillingPeriodRequestDto request);
        Task<BillingPeriodResponseDto?> ReopenAsync(int billingPeriodId);
    }
}