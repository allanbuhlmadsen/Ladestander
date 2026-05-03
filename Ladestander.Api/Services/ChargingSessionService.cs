using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services.Interfaces;
using Ladestander.Api.Entities;

namespace Ladestander.Api.Services
{
    public class ChargingSessionService : IChargingSessionService
    {
        private readonly IChargingSessionRepository _chargingSessionRepository;
        private readonly IBillingPeriodRepository _billingPeriodRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public ChargingSessionService(
            IChargingSessionRepository chargingSessionRepository,
            IBillingPeriodRepository billingPeriodRepository,
            IInvoiceRepository invoiceRepository)
        {
            _chargingSessionRepository = chargingSessionRepository;
            _billingPeriodRepository = billingPeriodRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<List<ChargingSessionResponseDto>> GetAllAsync()
        {
            var sessions = await _chargingSessionRepository.GetAllAsync();

            return sessions.Select(s => new ChargingSessionResponseDto(
                s.ChargingSessionId,
                s.CustomerId,
                s.BillingPeriodId,
                s.InvoiceId,
                s.ChargerAlias,
                s.StartTime,
                s.EnergyKWh,
                s.SourceUserName
            )).ToList();
        }

        public async Task<List<ChargingSessionResponseDto>> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId)
        {
            var sessions = await _chargingSessionRepository
                .GetByCustomerAndPeriodAsync(customerId, billingPeriodId);

            return sessions.Select(s => new ChargingSessionResponseDto(
                s.ChargingSessionId,
                s.CustomerId,
                s.BillingPeriodId,
                s.InvoiceId,
                s.ChargerAlias,
                s.StartTime,
                s.EnergyKWh,
                s.SourceUserName
            )).ToList();
        }

        public async Task<ChargingSessionResponseDto?> GetByIdAsync(int chargingSessionId)
        {
            var session = await _chargingSessionRepository.GetByIdAsync(chargingSessionId);

            if (session is null)
            {
                return null;
            }

            return new ChargingSessionResponseDto(
                session.ChargingSessionId,
                session.CustomerId,
                session.BillingPeriodId,
                session.InvoiceId,
                session.ChargerAlias,
                session.StartTime,
                session.EnergyKWh,
                session.SourceUserName
            );
        }

        public async Task<ChargingSessionResponseDto> ImportAsync(ChargingSessionImportRequestDto request)
        {
            var billingPeriod = await _billingPeriodRepository.GetByIdAsync(request.BillingPeriodId);

            if (billingPeriod is null)
            {
                throw new InvalidOperationException($"BillingPeriod with id {request.BillingPeriodId} was not found.");
            }

            if (billingPeriod.IsClosed)
            {
                throw new InvalidOperationException("ChargingSession cannot be imported into a closed BillingPeriod.");
            }

            bool exists = await _chargingSessionRepository.ExistsAsync(
                request.CustomerId,
                request.BillingPeriodId,
                request.StartTime,
                request.ChargerAlias
            );

            if (exists)
            {
                throw new InvalidOperationException("ChargingSession already exists for the given Customer, BillingPeriod, StartTime and ChargerAlias.");
            }

            var chargingSession = new ChargingSession
            {
                CustomerId = request.CustomerId,
                BillingPeriodId = request.BillingPeriodId,
                InvoiceId = null,
                ChargerAlias = request.ChargerAlias,
                StartTime = request.StartTime,
                EnergyKWh = request.EnergyKWh,
                SourceUserName = request.SourceUserName
            };

            var created = await _chargingSessionRepository.AddAsync(chargingSession);

            return new ChargingSessionResponseDto(
                created.ChargingSessionId,
                created.CustomerId,
                created.BillingPeriodId,
                created.InvoiceId,
                created.ChargerAlias,
                created.StartTime,
                created.EnergyKWh,
                created.SourceUserName
            );
        }
    }
}