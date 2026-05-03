namespace Ladestander.Api.Services.Interfaces
{
    public interface IInvoiceCalculationService
    {
        decimal CalculateTotalEnergyKWh(IEnumerable<decimal> energyValues);
        decimal CalculateTotalAmount(decimal totalEnergyKWh, decimal averagePriceKWh);
    }
}