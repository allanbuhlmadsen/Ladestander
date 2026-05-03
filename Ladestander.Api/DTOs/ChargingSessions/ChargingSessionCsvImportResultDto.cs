namespace Ladestander.Api.DTOs.ChargingSessions;

public record ChargingSessionCsvImportResultDto(
    int ImportedCount,
    int SkippedCount,
    List<string> Errors
);