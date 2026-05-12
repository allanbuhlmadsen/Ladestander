namespace Ladestander.Web.DTOs;

public record ChargingSessionImportRequestDto(
    int CustomerId,
    int BillingPeriodId,
    string? ChargerAlias,
    DateTime? StartTime,
    decimal EnergyKWh,
    string? SourceUserName
);