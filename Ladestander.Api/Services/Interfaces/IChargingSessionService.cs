using Ladestander.Api.DTOs.ChargingSessions;

namespace Ladestander.Api.Services.Interfaces
{
    public interface IChargingSessionService
    {
        Task<List<ChargingSessionResponseDto>> GetAllAsync();
        Task<List<ChargingSessionResponseDto>> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId);
        Task<ChargingSessionResponseDto?> GetByIdAsync(int chargingSessionId);
        Task<ChargingSessionResponseDto> ImportAsync(ChargingSessionImportRequestDto request);
    }
}