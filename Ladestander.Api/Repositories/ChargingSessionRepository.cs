using Ladestander.Api.Data;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Repositories
{
    public class ChargingSessionRepository : IChargingSessionRepository
    {
        private readonly AppDbContext _context;

        public ChargingSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChargingSession>> GetAllAsync()
        {
            return await _context.ChargingSessions
                .ToListAsync();
        }

        public async Task<List<ChargingSession>> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId)
        {
            return await _context.ChargingSessions
                .Where(s => s.CustomerId == customerId &&
                            s.BillingPeriodId == billingPeriodId)
                .ToListAsync();
        }

        public async Task<ChargingSession?> GetByIdAsync(int chargingSessionId)
        {
            return await _context.ChargingSessions
                .FirstOrDefaultAsync(s => s.ChargingSessionId == chargingSessionId);
        }

        public async Task<ChargingSession> AddAsync(ChargingSession chargingSession)
        {
            _context.ChargingSessions.Add(chargingSession);
            await _context.SaveChangesAsync();
            return chargingSession;
        }

        public async Task UpdateRangeAsync(IEnumerable<ChargingSession> chargingSessions)
        {
            _context.ChargingSessions.UpdateRange(chargingSessions);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(
            int customerId,
            int billingPeriodId,
            DateTime? startTime,
            string? chargerAlias)
        {
            return await _context.ChargingSessions
                .AnyAsync(s =>
                    s.CustomerId == customerId &&
                    s.BillingPeriodId == billingPeriodId &&
                    s.StartTime == startTime &&
                    s.ChargerAlias == chargerAlias);
        }
    }
}