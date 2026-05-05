using Ladestander.Api.DTOs.BillingPeriods;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services;
using Moq;
using Xunit;

namespace Ladestander.Api.Tests.Services;

public class BillingPeriodServiceTests
{
    private readonly Mock<IBillingPeriodRepository> _billingPeriodRepositoryMock;

    private readonly BillingPeriodService _service;

    public BillingPeriodServiceTests()
    {
        _billingPeriodRepositoryMock = new Mock<IBillingPeriodRepository>();

        _service = new BillingPeriodService(
            _billingPeriodRepositoryMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenBillingPeriodDoesNotExist()
    {
        // Arrange
        var billingPeriodId = 9999;

        var request = CreateValidUpdateRequest();

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync((BillingPeriod?)null);

        // Act
        var result = await _service.UpdateAsync(billingPeriodId, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenBillingPeriodIsClosed()
    {
        // Arrange
        var billingPeriodId = 1;

        var request = CreateValidUpdateRequest();

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = billingPeriodId,
                IsClosed = true
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(billingPeriodId, request));

        // Assert
        Assert.Equal("BillingPeriod is closed and cannot be updated.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBillingPeriod_WhenBillingPeriodIsOpen()
    {
        // Arrange
        var billingPeriodId = 1;

        var request = CreateValidUpdateRequest();

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = billingPeriodId,
                IsClosed = false
            });

        _billingPeriodRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<BillingPeriod>()))
            .ReturnsAsync((BillingPeriod billingPeriod) => billingPeriod);

        // Act
        var result = await _service.UpdateAsync(billingPeriodId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(billingPeriodId, result.BillingPeriodId);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.MonthName, result.MonthName);
        Assert.Equal(request.MonthNumber, result.MonthNumber);
        Assert.Equal(request.Year, result.Year);
        Assert.Equal(request.AveragePriceKWh, result.AveragePriceKWh);
        Assert.Equal(request.AverageSolarPriceKWh, result.AverageSolarPriceKWh);
        Assert.Equal(request.IsElectricityBillUpdated, result.IsElectricityBillUpdated);
        Assert.Equal(request.IsClosed, result.IsClosed);

        _billingPeriodRepositoryMock.Verify(
            repo => repo.UpdateAsync(It.IsAny<BillingPeriod>()),
            Times.Once);
    }

    [Fact]
    public async Task ReopenAsync_ReturnsNull_WhenBillingPeriodDoesNotExist()
    {
        // Arrange
        var billingPeriodId = 9999;

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync((BillingPeriod?)null);

        // Act
        var result = await _service.ReopenAsync(billingPeriodId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReopenAsync_SetsIsClosedToFalse_WhenBillingPeriodExists()
    {
        // Arrange
        var billingPeriodId = 1;

        var billingPeriod = new BillingPeriod
        {
            BillingPeriodId = billingPeriodId,
            IsClosed = true,
            Name = "Test",
            MonthName = "Januar",
            MonthNumber = 1,
            Year = 2026
        };

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(billingPeriod);

        _billingPeriodRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<BillingPeriod>()))
            .ReturnsAsync((BillingPeriod bp) => bp);

        // Act
        var result = await _service.ReopenAsync(billingPeriodId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsClosed);

        _billingPeriodRepositoryMock.Verify(
            repo => repo.UpdateAsync(It.Is<BillingPeriod>(bp => bp.IsClosed == false)),
            Times.Once);
    }



    private static UpdateBillingPeriodRequestDto CreateValidUpdateRequest()
    {
        return new UpdateBillingPeriodRequestDto(
            Name: "Testperiode",
            MonthName: "Januar",
            MonthNumber: 1,
            Year: 2026,
            TransportEnergyNorth: 0,
            TransportNetwork: 0,
            TransportEnergySouth: 0,
            ElectricityTax: 0,
            VariablePrice: 2.5m,
            GreenElectricity: 0,
            ClimateContribution: 0,
            TsoSubscription: 0,
            NetworkSubscription: 0,
            SupplySubscription: 0,
            NetsFee: 0,
            MonthlyConsumption: 100,
            SolarProduction: 0,
            SolarConsumption: 0,
            AveragePriceKWh: 2.5m,
            AverageSolarPriceKWh: 0,
            IsElectricityBillUpdated: true,
            IsClosed: false
        );
    }
}