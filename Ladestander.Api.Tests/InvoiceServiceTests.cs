using Ladestander.Api.DTOs.Invoices;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services;
using Ladestander.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace Ladestander.Api.Tests;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IChargingSessionRepository> _chargingSessionRepositoryMock;
    private readonly Mock<IBillingPeriodRepository> _billingPeriodRepositoryMock;
    private readonly Mock<IInvoiceCalculationService> _invoiceCalculationServiceMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;

    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _chargingSessionRepositoryMock = new Mock<IChargingSessionRepository>();
        _billingPeriodRepositoryMock = new Mock<IBillingPeriodRepository>();
        _invoiceCalculationServiceMock = new Mock<IInvoiceCalculationService>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _service = new InvoiceService(
            _invoiceRepositoryMock.Object,
            _chargingSessionRepositoryMock.Object,
            _billingPeriodRepositoryMock.Object,
            _invoiceCalculationServiceMock.Object,
            _customerRepositoryMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsInvalidOperationException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var request = new GenerateInvoiceRequestDto(
            CustomerId: 9999,
            BillingPeriodId: 1);

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.CustomerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(request));

        // Assert
        Assert.Equal("Customer with id 9999 was not found.", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsInvalidOperationException_WhenBillingPeriodDoesNotExist()
    {
        // Arrange
        var request = new GenerateInvoiceRequestDto(
            CustomerId: 1,
            BillingPeriodId: 9999);

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.CustomerId))
            .ReturnsAsync(new Customer { CustomerId = request.CustomerId });

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync((BillingPeriod?)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(request));

        // Assert
        Assert.Equal("BillingPeriod with id 9999 was not found.", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsInvalidOperationException_WhenBillingPeriodIsClosed()
    {
        // Arrange
        var request = new GenerateInvoiceRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1);

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.CustomerId))
            .ReturnsAsync(new Customer { CustomerId = request.CustomerId });

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                IsClosed = true
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(request));

        // Assert
        Assert.Equal("Invoice cannot be generated for a closed BillingPeriod.", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsInvalidOperationException_WhenInvoiceAlreadyExists()
    {
        // Arrange
        var request = new GenerateInvoiceRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1);

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.CustomerId))
            .ReturnsAsync(new Customer { CustomerId = request.CustomerId });

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                IsClosed = false
            });

        _invoiceRepositoryMock
            .Setup(repo => repo.ExistsAsync(request.CustomerId, request.BillingPeriodId))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(request));

        // Assert
        Assert.Equal("Invoice already exists for the selected Customer and BillingPeriod.", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsInvalidOperationException_WhenNoChargingSessionsExist()
    {
        // Arrange
        var request = new GenerateInvoiceRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1);

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.CustomerId))
            .ReturnsAsync(new Customer { CustomerId = request.CustomerId });

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                IsClosed = false
            });

        _invoiceRepositoryMock
            .Setup(repo => repo.ExistsAsync(request.CustomerId, request.BillingPeriodId))
            .ReturnsAsync(false);

        _chargingSessionRepositoryMock
            .Setup(repo => repo.GetByCustomerAndPeriodAsync(request.CustomerId, request.BillingPeriodId))
            .ReturnsAsync(new List<ChargingSession>());

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(request));

        // Assert
        Assert.Equal("No charging sessions found for the selected Customer and BillingPeriod.", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_CreatesDraftInvoiceAndLinksChargingSessions_WhenRequestIsValid()
    {
        // Arrange
        var request = new GenerateInvoiceRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1);

        var chargingSessions = new List<ChargingSession>
        {
            new ChargingSession { ChargingSessionId = 1, EnergyKWh = 10 },
            new ChargingSession { ChargingSessionId = 2, EnergyKWh = 20 }
        };

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.CustomerId))
            .ReturnsAsync(new Customer { CustomerId = request.CustomerId });

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                AveragePriceKWh = 2.5m,
                IsClosed = false
            });

        _invoiceRepositoryMock
            .Setup(repo => repo.ExistsAsync(request.CustomerId, request.BillingPeriodId))
            .ReturnsAsync(false);

        _chargingSessionRepositoryMock
            .Setup(repo => repo.GetByCustomerAndPeriodAsync(request.CustomerId, request.BillingPeriodId))
            .ReturnsAsync(chargingSessions);

        _invoiceCalculationServiceMock
            .Setup(service => service.CalculateTotalEnergyKWh(It.IsAny<IEnumerable<decimal>>()))
            .Returns(30m);

        _invoiceCalculationServiceMock
            .Setup(service => service.CalculateTotalAmount(30m, 2.5m))
            .Returns(75m);

        _invoiceRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice invoice) =>
            {
                invoice.InvoiceId = 100;
                return invoice;
            });

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.Equal(100, result.InvoiceId);
        Assert.Equal(request.CustomerId, result.CustomerId);
        Assert.Equal(request.BillingPeriodId, result.BillingPeriodId);
        Assert.Equal("Draft", result.Status);
        Assert.False(result.IsSent);
        Assert.False(result.IsPaid);
        Assert.Equal(30m, result.TotalEnergyKWh);
        Assert.Equal(75m, result.TotalAmount);

        Assert.All(chargingSessions, chargingSession =>
            Assert.Equal(100, chargingSession.InvoiceId));

        _chargingSessionRepositoryMock.Verify(
            repo => repo.UpdateRangeAsync(chargingSessions),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsNull_WhenInvoiceDoesNotExist()
    {
        // Arrange
        var invoiceId = 9999;

        var request = new UpdateInvoiceStatusRequestDto("Sent");

        _invoiceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync((Invoice?)null);

        // Act
        var result = await _service.UpdateStatusAsync(invoiceId, request);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("SomethingElse")]
    [InlineData("Cancelled")]
    public async Task UpdateStatusAsync_ThrowsInvalidOperationException_WhenStatusIsInvalid(string status)
    {
        // Arrange
        var invoiceId = 1;

        var request = new UpdateInvoiceStatusRequestDto(status);

        _invoiceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync(new Invoice
            {
                InvoiceId = invoiceId,
                Status = "Draft",
                IsSent = false,
                IsPaid = false
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateStatusAsync(invoiceId, request));

        // Assert
        Assert.Equal("Invoice status must be Draft, Sent or Paid.", exception.Message);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Sent")]
    [InlineData("Paid")]
    public async Task UpdateStatusAsync_ThrowsInvalidOperationException_WhenInvoiceIsPaid(string newStatus)
    {
        // Arrange
        var invoiceId = 1;

        var request = new UpdateInvoiceStatusRequestDto(newStatus);

        _invoiceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync(new Invoice
            {
                InvoiceId = invoiceId,
                Status = "Paid",
                IsSent = true,
                IsPaid = true
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateStatusAsync(invoiceId, request));

        // Assert
        Assert.Equal("Paid invoices cannot be changed.", exception.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_ThrowsInvalidOperationException_WhenSentInvoiceIsChangedBackToDraft()
    {
        // Arrange
        var invoiceId = 1;

        var request = new UpdateInvoiceStatusRequestDto("Draft");

        _invoiceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync(new Invoice
            {
                InvoiceId = invoiceId,
                Status = "Sent",
                IsSent = true,
                IsPaid = false
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateStatusAsync(invoiceId, request));

        // Assert
        Assert.Equal("Sent invoices cannot be changed back to Draft.", exception.Message);
    }

    [Theory]
    [InlineData("Draft", false, false)]
    [InlineData("Sent", true, false)]
    [InlineData("Paid", true, true)]
    public async Task UpdateStatusAsync_UpdatesInvoiceStatus_WhenStatusIsValid(
    string newStatus,
    bool expectedIsSent,
    bool expectedIsPaid)
    {
        // Arrange
        var invoiceId = 1;

        var request = new UpdateInvoiceStatusRequestDto(newStatus);

        _invoiceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync(new Invoice
            {
                InvoiceId = invoiceId,
                CustomerId = 1,
                BillingPeriodId = 1,
                Status = "Draft",
                IsSent = false,
                IsPaid = false
            });

        _invoiceRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice invoice) => invoice);

        // Act
        var result = await _service.UpdateStatusAsync(invoiceId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newStatus, result.Status);
        Assert.Equal(expectedIsSent, result.IsSent);
        Assert.Equal(expectedIsPaid, result.IsPaid);

        _invoiceRepositoryMock.Verify(
            repo => repo.UpdateAsync(It.IsAny<Invoice>()),
            Times.Once);
    }
}