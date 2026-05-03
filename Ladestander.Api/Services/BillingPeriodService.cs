using Ladestander.Api.DTOs.BillingPeriods;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services.Interfaces;

namespace Ladestander.Api.Services
{
    public class BillingPeriodService : IBillingPeriodService
    {
        private readonly IBillingPeriodRepository _billingPeriodRepository;

        public BillingPeriodService(IBillingPeriodRepository billingPeriodRepository)
        {
            _billingPeriodRepository = billingPeriodRepository;
        }

        public async Task<List<BillingPeriodResponseDto>> GetAllAsync()
        {
            var periods = await _billingPeriodRepository.GetAllAsync();

            return periods.Select(p => new BillingPeriodResponseDto(
                p.BillingPeriodId,
                p.Name,
                p.MonthName,
                p.MonthNumber,
                p.Year,
                p.AveragePriceKWh,
                p.AverageSolarPriceKWh,
                p.IsElectricityBillUpdated,
                p.IsClosed
            )).ToList();
        }

        public async Task<BillingPeriodResponseDto?> GetByIdAsync(int billingPeriodId)
        {
            var period = await _billingPeriodRepository.GetByIdAsync(billingPeriodId);

            if (period is null)
            {
                return null;
            }

            return new BillingPeriodResponseDto(
                period.BillingPeriodId,
                period.Name,
                period.MonthName,
                period.MonthNumber,
                period.Year,
                period.AveragePriceKWh,
                period.AverageSolarPriceKWh,
                period.IsElectricityBillUpdated,
                period.IsClosed
            );
        }

        public async Task<BillingPeriodResponseDto> CreateAsync(CreateBillingPeriodRequestDto request)
        {
            var billingPeriod = new BillingPeriod
            {
                Name = request.Name,
                MonthName = request.MonthName,
                MonthNumber = request.MonthNumber,
                Year = request.Year,

                TransportEnergyNorth = request.TransportEnergyNorth,
                TransportNetwork = request.TransportNetwork,
                TransportEnergySouth = request.TransportEnergySouth,

                ElectricityTax = request.ElectricityTax,
                VariablePrice = request.VariablePrice,
                GreenElectricity = request.GreenElectricity,
                ClimateContribution = request.ClimateContribution,

                TsoSubscription = request.TsoSubscription,
                NetworkSubscription = request.NetworkSubscription,
                SupplySubscription = request.SupplySubscription,

                NetsFee = request.NetsFee,
                MonthlyConsumption = request.MonthlyConsumption,

                SolarProduction = request.SolarProduction,
                SolarConsumption = request.SolarConsumption,

                AveragePriceKWh = request.AveragePriceKWh,
                AverageSolarPriceKWh = request.AverageSolarPriceKWh,

                IsElectricityBillUpdated = request.IsElectricityBillUpdated,
                IsClosed = request.IsClosed
            };

            var created = await _billingPeriodRepository.AddAsync(billingPeriod);

            return new BillingPeriodResponseDto(
                created.BillingPeriodId,
                created.Name,
                created.MonthName,
                created.MonthNumber,
                created.Year,
                created.AveragePriceKWh,
                created.AverageSolarPriceKWh,
                created.IsElectricityBillUpdated,
                created.IsClosed
            );
        }

        public async Task<BillingPeriodResponseDto?> UpdateAsync(int billingPeriodId, UpdateBillingPeriodRequestDto request)
        {
            var billingPeriod = await _billingPeriodRepository.GetByIdAsync(billingPeriodId);

            if (billingPeriod is not null && billingPeriod.IsClosed)
            {
                throw new InvalidOperationException("BillingPeriod is closed and cannot be updated.");
            }

            if (billingPeriod is null)
            {
                return null;
            }

            billingPeriod.Name = request.Name;
            billingPeriod.MonthName = request.MonthName;
            billingPeriod.MonthNumber = request.MonthNumber;
            billingPeriod.Year = request.Year;

            billingPeriod.TransportEnergyNorth = request.TransportEnergyNorth;
            billingPeriod.TransportNetwork = request.TransportNetwork;
            billingPeriod.TransportEnergySouth = request.TransportEnergySouth;

            billingPeriod.ElectricityTax = request.ElectricityTax;
            billingPeriod.VariablePrice = request.VariablePrice;
            billingPeriod.GreenElectricity = request.GreenElectricity;
            billingPeriod.ClimateContribution = request.ClimateContribution;

            billingPeriod.TsoSubscription = request.TsoSubscription;
            billingPeriod.NetworkSubscription = request.NetworkSubscription;
            billingPeriod.SupplySubscription = request.SupplySubscription;

            billingPeriod.NetsFee = request.NetsFee;
            billingPeriod.MonthlyConsumption = request.MonthlyConsumption;

            billingPeriod.SolarProduction = request.SolarProduction;
            billingPeriod.SolarConsumption = request.SolarConsumption;

            billingPeriod.AveragePriceKWh = request.AveragePriceKWh;
            billingPeriod.AverageSolarPriceKWh = request.AverageSolarPriceKWh;

            billingPeriod.IsElectricityBillUpdated = request.IsElectricityBillUpdated;
            billingPeriod.IsClosed = request.IsClosed;

            var updated = await _billingPeriodRepository.UpdateAsync(billingPeriod);

            return new BillingPeriodResponseDto(
                updated.BillingPeriodId,
                updated.Name,
                updated.MonthName,
                updated.MonthNumber,
                updated.Year,
                updated.AveragePriceKWh,
                updated.AverageSolarPriceKWh,
                updated.IsElectricityBillUpdated,
                updated.IsClosed
            );
        }

        public async Task<BillingPeriodResponseDto?> ReopenAsync(int billingPeriodId)
        {
            var billingPeriod = await _billingPeriodRepository.GetByIdAsync(billingPeriodId);

            if (billingPeriod is null)
            {
                return null;
            }

            billingPeriod.IsClosed = false;

            var updatedBillingPeriod = await _billingPeriodRepository.UpdateAsync(billingPeriod);

            return new BillingPeriodResponseDto(
                updatedBillingPeriod.BillingPeriodId,
                updatedBillingPeriod.Name,
                updatedBillingPeriod.MonthName,
                updatedBillingPeriod.MonthNumber,
                updatedBillingPeriod.Year,
                updatedBillingPeriod.AveragePriceKWh,
                updatedBillingPeriod.AverageSolarPriceKWh,
                updatedBillingPeriod.IsElectricityBillUpdated,
                updatedBillingPeriod.IsClosed
            );
        }
    }
}