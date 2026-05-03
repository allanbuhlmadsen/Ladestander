namespace Ladestander.Api.DTOs.ChargingSessions;

public record ChargingSessionCsvRowDto(
    string SessionId,
    string ChargerAlias,
    DateTime StartTime,
    DateTime EndTime,
    decimal EnergyKWh,
    string UserName
);