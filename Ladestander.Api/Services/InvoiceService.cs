 using Ladestander.Api.DTOs.Invoices;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services.Interfaces;

namespace Ladestander.Api.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IChargingSessionRepository _chargingSessionRepository;
        private readonly IBillingPeriodRepository _billingPeriodRepository;
        private readonly IInvoiceCalculationService _invoiceCalculationService;
        private readonly ICustomerRepository _customerRepository;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IChargingSessionRepository chargingSessionRepository,
            IBillingPeriodRepository billingPeriodRepository,
            IInvoiceCalculationService invoiceCalculationService,
            ICustomerRepository customerRepository)
        {
            _invoiceRepository = invoiceRepository;
            _chargingSessionRepository = chargingSessionRepository;
            _billingPeriodRepository = billingPeriodRepository;
            _invoiceCalculationService = invoiceCalculationService;
            _customerRepository = customerRepository;
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();

            return invoices.Select(i => new InvoiceResponseDto(
                i.InvoiceId,
                i.OldInvoiceId,
                i.CustomerId,
                i.BillingPeriodId,
                i.InvoiceNumber,
                i.IsPaid,
                i.IsSent,
                i.Status,
                i.TotalEnergyKWh,
                i.TotalAmount,
                i.CreatedAt
            )).ToList();
        }

        public async Task<InvoiceResponseDto?> GetByIdAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);

            if (invoice is null)
            {
                return null;
            }

            return new InvoiceResponseDto(
                invoice.InvoiceId,
                invoice.OldInvoiceId,
                invoice.CustomerId,
                invoice.BillingPeriodId,
                invoice.InvoiceNumber,
                invoice.IsPaid,
                invoice.IsSent,
                invoice.Status,
                invoice.TotalEnergyKWh,
                invoice.TotalAmount,
                invoice.CreatedAt
            );
        }

        public async Task<InvoiceResponseDto> GenerateAsync(GenerateInvoiceRequestDto request)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);

            if (customer is null)
            {
                throw new InvalidOperationException($"Customer with id {request.CustomerId} was not found.");
            }

            var billingPeriod = await _billingPeriodRepository
                .GetByIdAsync(request.BillingPeriodId);

            if (billingPeriod is null)
            {
                throw new InvalidOperationException($"BillingPeriod with id {request.BillingPeriodId} was not found.");
            }

            if (billingPeriod.IsClosed)
            {
                throw new InvalidOperationException("Invoice cannot be generated for a closed BillingPeriod.");
            }

            var invoiceAlreadyExists = await _invoiceRepository
                .ExistsAsync(request.CustomerId, request.BillingPeriodId);

            if (invoiceAlreadyExists)
            {
                throw new InvalidOperationException("Invoice already exists for the selected Customer and BillingPeriod.");
            }

            var chargingSessions = await _chargingSessionRepository
                .GetByCustomerAndPeriodAsync(request.CustomerId, request.BillingPeriodId);

            if (chargingSessions is null || !chargingSessions.Any())
            {
                throw new InvalidOperationException("No charging sessions found for the selected Customer and BillingPeriod.");
            }

            var totalEnergyKWh = _invoiceCalculationService
                .CalculateTotalEnergyKWh(chargingSessions.Select(cs => cs.EnergyKWh));

            var totalAmount = _invoiceCalculationService
                .CalculateTotalAmount(totalEnergyKWh, billingPeriod.AveragePriceKWh);

            // Store calculated totals on the invoice as a historical snapshot.
            // This preserves invoice consistency even if billing prices or charging sessions change later.
            var invoice = new Invoice
            {
                CustomerId = request.CustomerId,
                BillingPeriodId = request.BillingPeriodId,
                InvoiceNumber = null,
                IsPaid = false,
                IsSent = false,
                Status = "Draft",
                TotalEnergyKWh = totalEnergyKWh,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow
            };

            var createdInvoice = await _invoiceRepository.AddAsync(invoice);

            // Link the charging sessions to the generated invoice after the invoice has been persisted.
            foreach (var chargingSession in chargingSessions)
            {
                chargingSession.InvoiceId = createdInvoice.InvoiceId;
            }

            await _chargingSessionRepository.UpdateRangeAsync(chargingSessions);

            return new InvoiceResponseDto(
                createdInvoice.InvoiceId,
                createdInvoice.OldInvoiceId,
                createdInvoice.CustomerId,
                createdInvoice.BillingPeriodId,
                createdInvoice.InvoiceNumber,
                createdInvoice.IsPaid,
                createdInvoice.IsSent,
                createdInvoice.Status,
                createdInvoice.TotalEnergyKWh,
                createdInvoice.TotalAmount,
                createdInvoice.CreatedAt
            );
        }

        public async Task<InvoiceResponseDto?> UpdateStatusAsync(int invoiceId, UpdateInvoiceStatusRequestDto request)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);

            if (invoice is null)
            {
                return null;
            }

            var normalizedStatus = request.Status.Trim();

            if (normalizedStatus != "Draft" &&
                normalizedStatus != "Sent" &&
                normalizedStatus != "Paid")
            {
                throw new InvalidOperationException("Invoice status must be Draft, Sent or Paid.");
            }

            // Invoice status transitions are intentionally restricted.
            // Paid invoices are final, and sent invoices cannot be moved back to Draft.
            if (invoice.IsPaid)
            {
                throw new InvalidOperationException("Paid invoices cannot be changed.");
            }

            if (invoice.IsSent && !invoice.IsPaid && normalizedStatus == "Draft")
            {
                throw new InvalidOperationException("Sent invoices cannot be changed back to Draft.");
            }

            invoice.Status = normalizedStatus;
            invoice.IsSent = normalizedStatus == "Sent" || normalizedStatus == "Paid";
            invoice.IsPaid = normalizedStatus == "Paid";

            var updatedInvoice = await _invoiceRepository.UpdateAsync(invoice);

            return new InvoiceResponseDto(
                updatedInvoice.InvoiceId,
                updatedInvoice.OldInvoiceId,
                updatedInvoice.CustomerId,
                updatedInvoice.BillingPeriodId,
                updatedInvoice.InvoiceNumber,
                updatedInvoice.IsPaid,
                updatedInvoice.IsSent,
                updatedInvoice.Status,
                updatedInvoice.TotalEnergyKWh,
                updatedInvoice.TotalAmount,
                updatedInvoice.CreatedAt
            );
        }

        public async Task<InvoiceResponseDto?> GetByCustomerAndPeriodAsync(int customerId, int billingPeriodId)
        {
            var invoice = await _invoiceRepository.GetByCustomerAndPeriodAsync(customerId, billingPeriodId);

            if (invoice is null)
            {
                return null;
            }

            return new InvoiceResponseDto(
                invoice.InvoiceId,
                invoice.OldInvoiceId,
                invoice.CustomerId,
                invoice.BillingPeriodId,
                invoice.InvoiceNumber,
                invoice.IsPaid,
                invoice.IsSent,
                invoice.Status,
                invoice.TotalEnergyKWh,
                invoice.TotalAmount,
                invoice.CreatedAt
            );
        }
    }
}