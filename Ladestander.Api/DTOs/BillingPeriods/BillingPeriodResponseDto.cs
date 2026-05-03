namespace Ladestander.Api.DTOs.BillingPeriods;

public record BillingPeriodResponseDto(
    int BillingPeriodId,
    string Name,
    string MonthName,
    int MonthNumber,
    int Year,
    decimal AveragePriceKWh,
    decimal AverageSolarPriceKWh,
    bool IsElectricityBillUpdated,
    bool IsClosed
);