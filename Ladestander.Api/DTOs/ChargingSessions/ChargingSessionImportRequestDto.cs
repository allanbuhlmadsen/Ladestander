using System.ComponentModel.DataAnnotations;

namespace Ladestander.Api.DTOs.ChargingSessions;

public record ChargingSessionImportRequestDto(
    [Range(1, int.MaxValue)]
    int CustomerId,

    [Range(1, int.MaxValue)]
    int BillingPeriodId,

    [MaxLength(255)]
    string? ChargerAlias,

    DateTime? StartTime,

    [Range(0, double.MaxValue)]
    decimal EnergyKWh,

    [MaxLength(255)]
    string? SourceUserName
);