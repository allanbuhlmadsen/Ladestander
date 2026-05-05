using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services;
using Moq;
using Xunit;

namespace Ladestander.Api.Tests;

public class ChargingSessionServiceTests
{
    private readonly Mock<IChargingSessionRepository> _chargingSessionRepositoryMock;
    private readonly Mock<IBillingPeriodRepository> _billingPeriodRepositoryMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;

    private readonly ChargingSessionService _service;

    public ChargingSessionServiceTests()
    {
        _chargingSessionRepositoryMock = new Mock<IChargingSessionRepository>();
        _billingPeriodRepositoryMock = new Mock<IBillingPeriodRepository>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();

        _service = new ChargingSessionService(
            _chargingSessionRepositoryMock.Object,
            _billingPeriodRepositoryMock.Object,
            _invoiceRepositoryMock.Object);
    }

    [Fact]
    public async Task ImportAsync_ThrowsInvalidOperationException_WhenBillingPeriodDoesNotExist()
    {
        // Arrange
        var request = new ChargingSessionImportRequestDto(
            CustomerId: 1,
            BillingPeriodId: 9999,
            ChargerAlias: "Test",
            StartTime: DateTime.UtcNow,
            EnergyKWh: 10,
            SourceUserName: "Test User"
        );

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync((BillingPeriod?)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ImportAsync(request));

        // Assert
        Assert.Equal($"BillingPeriod with id {request.BillingPeriodId} was not found.", exception.Message);
    }

    [Fact]
    public async Task ImportAsync_ThrowsInvalidOperationException_WhenBillingPeriodIsClosed()
    {
        // Arrange
        var request = new ChargingSessionImportRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1,
            ChargerAlias: "Test",
            StartTime: DateTime.UtcNow,
            EnergyKWh: 10,
            SourceUserName: "Test User"
        );

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                IsClosed = true
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ImportAsync(request));

        // Assert
        Assert.Equal("ChargingSession cannot be imported into a closed BillingPeriod.", exception.Message);
    }

    [Fact]
    public async Task ImportAsync_ThrowsInvalidOperationException_WhenChargingSessionAlreadyExists()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        var request = new ChargingSessionImportRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1,
            ChargerAlias: "Test",
            StartTime: startTime,
            EnergyKWh: 10,
            SourceUserName: "Test User"
        );

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                IsClosed = false
            });

        _chargingSessionRepositoryMock
            .Setup(repo => repo.ExistsAsync(
                request.CustomerId,
                request.BillingPeriodId,
                request.StartTime,
                request.ChargerAlias))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ImportAsync(request));

        // Assert
        Assert.Equal(
            "ChargingSession already exists for the given Customer, BillingPeriod, StartTime and ChargerAlias.",
            exception.Message);
    }

    [Fact]
    public async Task ImportAsync_CreatesChargingSession_WhenRequestIsValid()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        var request = new ChargingSessionImportRequestDto(
            CustomerId: 1,
            BillingPeriodId: 1,
            ChargerAlias: "Lader 1",
            StartTime: startTime,
            EnergyKWh: 12.345m,
            SourceUserName: "Test User"
        );

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.BillingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = request.BillingPeriodId,
                IsClosed = false
            });

        _chargingSessionRepositoryMock
            .Setup(repo => repo.ExistsAsync(
                request.CustomerId,
                request.BillingPeriodId,
                request.StartTime,
                request.ChargerAlias))
            .ReturnsAsync(false);

        _chargingSessionRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ChargingSession>()))
            .ReturnsAsync((ChargingSession chargingSession) =>
            {
                chargingSession.ChargingSessionId = 100;
                return chargingSession;
            });

        // Act
        var result = await _service.ImportAsync(request);

        // Assert
        Assert.Equal(100, result.ChargingSessionId);
        Assert.Equal(request.CustomerId, result.CustomerId);
        Assert.Equal(request.BillingPeriodId, result.BillingPeriodId);
        Assert.Null(result.InvoiceId);
        Assert.Equal(request.ChargerAlias, result.ChargerAlias);
        Assert.Equal(request.StartTime, result.StartTime);
        Assert.Equal(request.EnergyKWh, result.EnergyKWh);
        Assert.Equal(request.SourceUserName, result.SourceUserName);

        _chargingSessionRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<ChargingSession>(cs =>
                cs.CustomerId == request.CustomerId &&
                cs.BillingPeriodId == request.BillingPeriodId &&
                cs.InvoiceId == null &&
                cs.ChargerAlias == request.ChargerAlias &&
                cs.StartTime == request.StartTime &&
                cs.EnergyKWh == request.EnergyKWh &&
                cs.SourceUserName == request.SourceUserName)),
            Times.Once);
    }
}