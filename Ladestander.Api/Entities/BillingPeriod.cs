namespace Ladestander.Api.Entities;

public class BillingPeriod
{
    public int BillingPeriodId { get; set; }
    public int OldBillingPeriodId { get; set; }

    public string Name { get; set; } = null!;

    public string MonthName { get; set; } = null!;
    public int MonthNumber { get; set; }
    public int Year { get; set; }

    public decimal TransportEnergyNorth { get; set; }
    public decimal TransportNetwork { get; set; }
    public decimal TransportEnergySouth { get; set; }

    public decimal ElectricityTax { get; set; }
    public decimal VariablePrice { get; set; }
    public decimal GreenElectricity { get; set; }
    public decimal ClimateContribution { get; set; }

    public decimal TsoSubscription { get; set; }
    public decimal NetworkSubscription { get; set; }
    public decimal SupplySubscription { get; set; }

    public decimal NetsFee { get; set; }

    public decimal MonthlyConsumption { get; set; }

    public int SolarProduction { get; set; }
    public int SolarConsumption { get; set; }

    public decimal AveragePriceKWh { get; set; }
    public decimal AverageSolarPriceKWh { get; set; }

    public bool IsElectricityBillUpdated { get; set; }
    public bool IsClosed { get; set; }

    // Navigation
    public List<Invoice> Invoices { get; set; } = new();
    public List<ChargingSession> ChargingSessions { get; set; } = new();
}