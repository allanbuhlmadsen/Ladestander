using Ladestander.Api.Entities;

namespace Ladestander.Api.Repositories.Interfaces
{
    public interface IChargingSessionRepository
    {
        Task<List<ChargingSession>> GetAllAsync();
        Task<List<ChargingSession>> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId);
        Task<ChargingSession?> GetByIdAsync(int chargingSessionId);
        Task<ChargingSession> AddAsync(ChargingSession chargingSession);
        Task<bool> ExistsAsync(int customerId, int billingPeriodId, DateTime? startTime, string? chargerAlias);
        Task UpdateRangeAsync(IEnumerable<ChargingSession> chargingSessions);
    }
}