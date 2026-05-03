using System.ComponentModel.DataAnnotations;

namespace Ladestander.Api.DTOs.BillingPeriods;

public record UpdateBillingPeriodRequestDto(
    [Required]
    [MaxLength(255)]
    string Name,

    [Required]
    [MaxLength(50)]
    string MonthName,

    [Range(1, 12)]
    int MonthNumber,

    [Range(2000, 2100)]
    int Year,

    [Range(0, double.MaxValue)]
    decimal TransportEnergyNorth,

    [Range(0, double.MaxValue)]
    decimal TransportNetwork,

    [Range(0, double.MaxValue)]
    decimal TransportEnergySouth,

    [Range(0, double.MaxValue)]
    decimal ElectricityTax,

    [Range(0, double.MaxValue)]
    decimal VariablePrice,

    [Range(0, double.MaxValue)]
    decimal GreenElectricity,

    [Range(0, double.MaxValue)]
    decimal ClimateContribution,

    [Range(0, double.MaxValue)]
    decimal TsoSubscription,

    [Range(0, double.MaxValue)]
    decimal NetworkSubscription,

    [Range(0, double.MaxValue)]
    decimal SupplySubscription,

    [Range(0, double.MaxValue)]
    decimal NetsFee,

    [Range(0, double.MaxValue)]
    decimal MonthlyConsumption,

    [Range(0, int.MaxValue)]
    int SolarProduction,

    [Range(0, int.MaxValue)]
    int SolarConsumption,

    [Range(0, double.MaxValue)]
    decimal AveragePriceKWh,

    [Range(0, double.MaxValue)]
    decimal AverageSolarPriceKWh,

    bool IsElectricityBillUpdated,

    bool IsClosed
);