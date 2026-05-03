using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services.Interfaces;
using System.Globalization;

namespace Ladestander.Api.Services;

public class ChargingSessionCsvImportService : IChargingSessionCsvImportService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly IChargingSessionRepository _chargingSessionRepository;

    public ChargingSessionCsvImportService(
    ICustomerRepository customerRepository,
    IBillingPeriodRepository billingPeriodRepository,
    IChargingSessionRepository chargingSessionRepository)
    {
        _customerRepository = customerRepository;
        _billingPeriodRepository = billingPeriodRepository;
        _chargingSessionRepository = chargingSessionRepository;
    }

    public async Task<List<ChargingSessionCsvRowDto>> ParseAsync(IFormFile file)
    {
        var rows = new List<ChargingSessionCsvRowDto>();

        using var reader = new StreamReader(file.OpenReadStream());

        var headerLine = await reader.ReadLineAsync();

        string? line;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Contains(';')
                ? line.Split(';')
                : line.Contains('\t')
                    ? line.Split('\t')
                    : line.Split(',');

            if (columns.Length < 10)
            {
                continue;
            }

            var row = new ChargingSessionCsvRowDto(
                SessionId: columns[0].Trim('"'),
                ChargerAlias: columns[1].Trim('"'),
                StartTime: DateTime.ParseExact(columns[3].Trim('"'), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                EndTime: DateTime.ParseExact(columns[4].Trim('"'), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                EnergyKWh: decimal.Parse(columns[6].Trim('"'), CultureInfo.InvariantCulture),
                UserName: columns[9].Trim('"')
            );

            rows.Add(row);
        }

        return rows;
    }

    public async Task<ChargingSessionCsvImportResultDto> ImportAsync(IFormFile file, int billingPeriodId)
    {
        var errors = new List<string>();
        var importedCount = 0;
        var skippedCount = 0;

        var billingPeriod = await _billingPeriodRepository.GetByIdAsync(billingPeriodId);

        if (billingPeriod is null)
        {
            errors.Add($"BillingPeriod with id {billingPeriodId} was not found.");

            return new ChargingSessionCsvImportResultDto(
                ImportedCount: 0,
                SkippedCount: 0,
                Errors: errors
            );
        }

        if (billingPeriod.IsClosed)
        {
            errors.Add("ChargingSessions cannot be imported into a closed BillingPeriod.");

            return new ChargingSessionCsvImportResultDto(
                ImportedCount: 0,
                SkippedCount: 0,
                Errors: errors
            );
        }

        var rows = await ParseAsync(file);

        foreach (var row in rows)
        {
            var customer = await _customerRepository.GetByFullNameAsync(row.UserName);

            if (customer is null)
            {
                skippedCount++;
                errors.Add($"Customer '{row.UserName}' was not found.");
                continue;
            }

            var exists = await _chargingSessionRepository.ExistsAsync(
                customer.CustomerId,
                billingPeriodId,
                row.StartTime,
                row.ChargerAlias
            );

            if (exists)
            {
                skippedCount++;
                continue;
            }

            var chargingSession = new ChargingSession
            {
                CustomerId = customer.CustomerId,
                BillingPeriodId = billingPeriodId,
                InvoiceId = null,
                ChargerAlias = row.ChargerAlias,
                StartTime = row.StartTime,
                EnergyKWh = row.EnergyKWh,
                SourceUserName = row.UserName
            };

            await _chargingSessionRepository.AddAsync(chargingSession);

            importedCount++;
        }

        return new ChargingSessionCsvImportResultDto(
            ImportedCount: importedCount,
            SkippedCount: skippedCount,
            Errors: errors
        );
    }
}