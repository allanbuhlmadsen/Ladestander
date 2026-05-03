namespace Ladestander.Api.DTOs.ChargingSessions;

public record ChargingSessionResponseDto(
    int ChargingSessionId,
    int CustomerId,
    int BillingPeriodId,
    int? InvoiceId,
    string? ChargerAlias,
    DateTime? StartTime,
    decimal EnergyKWh,
    string? SourceUserName
);