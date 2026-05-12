namespace Ladestander.Web.DTOs;

public record ChargingSessionCsvImportResultDto(
    int ImportedCount,
    int SkippedCount,
    List<string> Errors
);