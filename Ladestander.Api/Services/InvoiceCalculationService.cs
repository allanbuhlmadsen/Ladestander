using Ladestander.Api.Services.Interfaces;

namespace Ladestander.Api.Services
{
    public class InvoiceCalculationService : IInvoiceCalculationService
    {
        public decimal CalculateTotalEnergyKWh(IEnumerable<decimal> energyValues)
        {
            return energyValues.Sum();
        }

        public decimal CalculateTotalAmount(decimal totalEnergyKWh, decimal averagePriceKWh)
        {
            return totalEnergyKWh * averagePriceKWh;
        }
    }
}