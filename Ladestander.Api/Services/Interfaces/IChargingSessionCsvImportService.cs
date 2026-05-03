using Ladestander.Api.DTOs.ChargingSessions;

namespace Ladestander.Api.Services.Interfaces;

public interface IChargingSessionCsvImportService
{
    Task<List<ChargingSessionCsvRowDto>> ParseAsync(IFormFile file);
    Task<ChargingSessionCsvImportResultDto> ImportAsync(IFormFile file, int billingPeriodId);
}