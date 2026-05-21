using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services.Interfaces;
using Ladestander.Api.Data;
using System.Globalization;

namespace Ladestander.Api.Services;

public class ChargingSessionCsvImportService : IChargingSessionCsvImportService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly IChargingSessionRepository _chargingSessionRepository;
    private readonly AppDbContext _dbContext;

    public ChargingSessionCsvImportService(
        ICustomerRepository customerRepository,
        IBillingPeriodRepository billingPeriodRepository,
        IChargingSessionRepository chargingSessionRepository,
        AppDbContext dbContext)
    {
        _customerRepository = customerRepository;
        _billingPeriodRepository = billingPeriodRepository;
        _chargingSessionRepository = chargingSessionRepository;
        _dbContext = dbContext;
    }

    public async Task<List<ChargingSessionCsvRowDto>> ParseAsync(IFormFile file)
    {
        var rows = new List<ChargingSessionCsvRowDto>();

        using var reader = new StreamReader(file.OpenReadStream());

        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return rows;
        }

        var headerDelimiter = DetectDelimiter(headerLine);

        var headers = headerLine
            .Split(headerDelimiter)
            .Select(CleanCsvValue)
            .ToList();

        // Use header-based mapping to support reordered ChargerSync CSV columns.
        var headerMap = headers
            .Select((header, index) => new { header, index })
            .ToDictionary(x => x.header, x => x.index);

        string? line;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var delimiter = DetectDelimiter(line);
            var columns = line.Split(delimiter);

            if (columns.Length < 10)
            {
                continue;
            }

            var row = new ChargingSessionCsvRowDto(
                SessionId: CleanCsvValue(columns[headerMap["Session ID"]]),
                ChargerAlias: CleanCsvValue(columns[headerMap["Charger Alias"]]),
                StartTime: DateTime.ParseExact(
                    CleanCsvValue(columns[headerMap["Start Time"]]),
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture),
                EndTime: DateTime.ParseExact(
                    CleanCsvValue(columns[headerMap["End Time"]]),
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture),
                EnergyKWh: decimal.Parse(
                    CleanCsvValue(columns[headerMap["Energy Delivered(kW·h)"]]),
                    CultureInfo.InvariantCulture),
                UserName: CleanCsvValue(columns[headerMap["User Name"]])
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

            // Prevent duplicate charging session imports for the same customer,
            // billing period, charger, and start time.
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

            // The raw CSV username is used for customer matching,
            // but the normalized customer name is stored for consistent long-term storage.
            var chargingSession = new ChargingSession
            {
                CustomerId = customer.CustomerId,
                BillingPeriodId = billingPeriodId,
                InvoiceId = null,
                ChargerAlias = row.ChargerAlias,
                StartTime = row.StartTime,
                EnergyKWh = row.EnergyKWh,
                SourceUserName = customer.FullName
            };

            await _chargingSessionRepository.AddAsync(chargingSession);

            importedCount++;
        }

        // Store import metadata and validation errors
        // to support traceability and troubleshooting.
        var importLog = new ImportLog
        {
            BillingPeriodId = billingPeriodId,
            ImportedAt = DateTime.UtcNow,
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            ErrorMessages = errors.Any()
                ? string.Join(" | ", errors.Distinct())
                : null,
            FileName = file.FileName
        };

        _dbContext.ImportLogs.Add(importLog);
        await _dbContext.SaveChangesAsync();

        return new ChargingSessionCsvImportResultDto(
            ImportedCount: importedCount,
            SkippedCount: skippedCount,
            Errors: errors
        );
    }

    private static char DetectDelimiter(string line)
    {
        if (line.Contains(';'))
        {
            return ';';
        }

        if (line.Contains('\t'))
        {
            return '\t';
        }

        return ',';
    }

    private static string CleanCsvValue(string value)
    {
        return value.Trim().Trim('"');
    }
}