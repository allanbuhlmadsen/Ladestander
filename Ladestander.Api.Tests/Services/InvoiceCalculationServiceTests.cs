using Ladestander.Api.Services;
using Xunit;

namespace Ladestander.Api.Tests.Services;

public class InvoiceCalculationServiceTests
{
    private readonly InvoiceCalculationService _service;

    public InvoiceCalculationServiceTests()
    {
        _service = new InvoiceCalculationService();
    }

    [Theory]
    [InlineData(10, 20, 30)]
    [InlineData(0, 20, 20)]
    [InlineData(1.5, 2.5, 4)]
    public void CalculateTotalEnergyKWh_ReturnsSum_WhenValuesProvided(
        decimal value1,
        decimal value2,
        decimal expected)
    {
        // Arrange
        var values = new List<decimal> { value1, value2 };

        // Act
        var result = _service.CalculateTotalEnergyKWh(values);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateTotalEnergyKWh_ReturnsZero_WhenNoValuesProvided()
    {
        // Arrange
        var values = new List<decimal>();

        // Act
        var result = _service.CalculateTotalEnergyKWh(values);

        // Assert
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(10, 2.5, 25)]
    [InlineData(0, 2.5, 0)]
    [InlineData(96.302, 2.1333, 205.4410566)]
    public void CalculateTotalAmount_ReturnsEnergyTimesPrice(
    decimal totalEnergyKWh,
    decimal averagePriceKWh,
    decimal expected)
    {
        // Act
        var result = _service.CalculateTotalAmount(totalEnergyKWh, averagePriceKWh);

        // Assert
        Assert.Equal(expected, result);
    }
}