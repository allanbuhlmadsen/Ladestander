namespace Ladestander.Web.DTOs;

public record UpdateBillingPeriodRequestDto(
    string Name,
    string MonthName,
    int MonthNumber,
    int Year,
    decimal TransportEnergyNorth,
    decimal TransportNetwork,
    decimal TransportEnergySouth,
    decimal ElectricityTax,
    decimal VariablePrice,
    decimal GreenElectricity,
    decimal ClimateContribution,
    decimal TsoSubscription,
    decimal NetworkSubscription,
    decimal SupplySubscription,
    decimal NetsFee,
    decimal MonthlyConsumption,
    int SolarProduction,
    int SolarConsumption,
    decimal AveragePriceKWh,
    decimal AverageSolarPriceKWh,
    bool IsElectricityBillUpdated,
    bool IsClosed
);