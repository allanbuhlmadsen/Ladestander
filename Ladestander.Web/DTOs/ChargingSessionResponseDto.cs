namespace Ladestander.Web.DTOs;

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