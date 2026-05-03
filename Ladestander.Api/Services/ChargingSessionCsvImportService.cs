using System.Globalization;
using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Services.Interfaces;

namespace Ladestander.Api.Services;

public class ChargingSessionCsvImportService : IChargingSessionCsvImportService
{
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
}