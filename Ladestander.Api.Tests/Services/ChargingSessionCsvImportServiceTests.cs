using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services;
using Microsoft.AspNetCore.Http;
using Ladestander.Api.Tests.TestHelpers;
using System.IO;
using Moq;
using Xunit;

namespace Ladestander.Api.Tests.Services;

public class ChargingSessionCsvImportServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IBillingPeriodRepository> _billingPeriodRepositoryMock;
    private readonly Mock<IChargingSessionRepository> _chargingSessionRepositoryMock;

    private readonly ChargingSessionCsvImportService _service;
    private readonly Ladestander.Api.Data.AppDbContext _dbContext;

    public ChargingSessionCsvImportServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _billingPeriodRepositoryMock = new Mock<IBillingPeriodRepository>();
        _chargingSessionRepositoryMock = new Mock<IChargingSessionRepository>();

        _dbContext = SqliteTestDbContextFactory.CreateContext();

        _service = new ChargingSessionCsvImportService(
            _customerRepositoryMock.Object,
            _billingPeriodRepositoryMock.Object,
            _chargingSessionRepositoryMock.Object,
            _dbContext);
    }

    [Fact]
    public async Task ImportAsync_ReturnsError_WhenBillingPeriodDoesNotExist()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();

        var billingPeriodId = 9999;

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync((BillingPeriod?)null);

        // Act
        var result = await _service.ImportAsync(fileMock.Object, billingPeriodId);

        // Assert
        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Contains(
            $"BillingPeriod with id {billingPeriodId} was not found.",
            result.Errors);
    }

    [Fact]
    public async Task ImportAsync_ReturnsError_WhenBillingPeriodIsClosed()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();

        var billingPeriodId = 1;

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = billingPeriodId,
                IsClosed = true
            });

        // Act
        var result = await _service.ImportAsync(fileMock.Object, billingPeriodId);

        // Assert
        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Contains(
            "ChargingSessions cannot be imported into a closed BillingPeriod.",
            result.Errors);
    }

    [Fact]
    public async Task ImportAsync_SkipsRowAndReturnsError_WhenCustomerDoesNotExist()
    {
        // Arrange
        var billingPeriodId = 1;

        _dbContext.BillingPeriods.Add(new BillingPeriod
        {
            BillingPeriodId = billingPeriodId,
            Name = "July 2024",
            MonthName = "July",
            MonthNumber = 7,
            Year = 2024,
            IsClosed = false
        });

        await _dbContext.SaveChangesAsync();

        var file = CreateCsvFile("""
        "Session ID","Charger Alias","SN","Start Time","End Time","Duration(h)","Energy Delivered(kW·h)","Total Cost(kr)","Session Status","User Name","Authorization Type","Stop Reason"
        "S1","Lader 1","SN1","30/07/2024 16:02","30/07/2024 18:02","2","10.500","0","Free session","Ukendt Kunde","RFID","EVDisconnected"
        """);

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = billingPeriodId,
                IsClosed = false
            });

        _customerRepositoryMock
            .Setup(repo => repo.GetByFullNameAsync("Ukendt Kunde"))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.ImportAsync(file, billingPeriodId);

        // Assert
        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Contains("Customer 'Ukendt Kunde' was not found.", result.Errors);
    }

    [Fact]
    public async Task ImportAsync_SkipsRow_WhenChargingSessionAlreadyExists()
    {
        // Arrange
        var billingPeriodId = 1;

        _dbContext.BillingPeriods.Add(new BillingPeriod
        {
            BillingPeriodId = billingPeriodId,
            Name = "July 2024",
            MonthName = "July",
            MonthNumber = 7,
            Year = 2024,
            IsClosed = false
        });

        await _dbContext.SaveChangesAsync();

        var file = CreateCsvFile("""
        "Session ID","Charger Alias","SN","Start Time","End Time","Duration(h)","Energy Delivered(kW·h)","Total Cost(kr)","Session Status","User Name","Authorization Type","Stop Reason"
        "S1","Lader 1","SN1","30/07/2024 16:02","30/07/2024 18:02","2","10.500","0","Free session","Test Kunde","RFID","EVDisconnected"
        """);

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = billingPeriodId,
                IsClosed = false
            });

        _customerRepositoryMock
            .Setup(repo => repo.GetByFullNameAsync("Test Kunde"))
            .ReturnsAsync(new Customer
            {
                CustomerId = 1,
                FirstName = "Test",
                LastName = "Kunde"
            });

        _chargingSessionRepositoryMock
            .Setup(repo => repo.ExistsAsync(
                1,
                billingPeriodId,
                new DateTime(2024, 7, 30, 16, 2, 0),
                "Lader 1"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ImportAsync(file, billingPeriodId);

        // Assert
        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ImportAsync_ImportsRow_WhenCsvRowIsValid()
    {
        // Arrange
        var billingPeriodId = 1;

        _dbContext.BillingPeriods.Add(new BillingPeriod
        {
            BillingPeriodId = billingPeriodId,
            Name = "July 2024",
            MonthName = "July",
            MonthNumber = 7,
            Year = 2024,
            IsClosed = false
        });

        await _dbContext.SaveChangesAsync();

        var file = CreateCsvFile("""
        "Session ID","Charger Alias","SN","Start Time","End Time","Duration(h)","Energy Delivered(kW·h)","Total Cost(kr)","Session Status","User Name","Authorization Type","Stop Reason"
        "S1","Lader 1","SN1","30/07/2024 16:02","30/07/2024 18:02","2","10.500","0","Free session","Test Kunde","RFID","EVDisconnected"
        """);

        _billingPeriodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(billingPeriodId))
            .ReturnsAsync(new BillingPeriod
            {
                BillingPeriodId = billingPeriodId,
                IsClosed = false
            });

        _customerRepositoryMock
            .Setup(repo => repo.GetByFullNameAsync("Test Kunde"))
            .ReturnsAsync(new Customer
            {
                CustomerId = 1,
                FirstName = "Test",
                LastName = "Kunde"
            });

        _chargingSessionRepositoryMock
            .Setup(repo => repo.ExistsAsync(
                1,
                billingPeriodId,
                new DateTime(2024, 7, 30, 16, 2, 0),
                "Lader 1"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ImportAsync(file, billingPeriodId);

        // Assert
        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Empty(result.Errors);

        _chargingSessionRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<ChargingSession>(cs =>
                cs.CustomerId == 1 &&
                cs.BillingPeriodId == billingPeriodId &&
                cs.InvoiceId == null &&
                cs.ChargerAlias == "Lader 1" &&
                cs.StartTime == new DateTime(2024, 7, 30, 16, 2, 0) &&
                cs.EnergyKWh == 10.500m &&
                cs.SourceUserName == "Test Kunde")),
            Times.Once);
    }

    [Fact]
    public async Task ParseAsync_ReturnsRows_WhenCsvIsValid()
    {
        // Arrange
        var file = CreateCsvFile("""
        "Session ID","Charger Alias","SN","Start Time","End Time","Duration(h)","Energy Delivered(kW·h)","Total Cost(kr)","Session Status","User Name","Authorization Type","Stop Reason"
        "S1","Lader 1","SN1","30/07/2024 16:02","30/07/2024 18:02","2","10.500","0","Free session","Test Kunde","RFID","EVDisconnected"
        """);

        // Act
        var result = await _service.ParseAsync(file);

        // Assert
        Assert.Single(result);

        var row = result[0];

        Assert.Equal("S1", row.SessionId);
        Assert.Equal("Lader 1", row.ChargerAlias);
        Assert.Equal(new DateTime(2024, 7, 30, 16, 2, 0), row.StartTime);
        Assert.Equal(new DateTime(2024, 7, 30, 18, 2, 0), row.EndTime);
        Assert.Equal(10.500m, row.EnergyKWh);
        Assert.Equal("Test Kunde", row.UserName);
    }

    [Fact]
    public async Task ParseAsync_ReturnsRow_WhenColumnsAreReordered()
    {
        // Arrange
        var file = CreateCsvFile("""
        "User Name","Energy Delivered(kW·h)","End Time","Start Time","Charger Alias","Session ID","SN","Duration(h)","Total Cost(kr)","Session Status","Authorization Type","Stop Reason"
        "Test Kunde","10.500","30/07/2024 18:02","30/07/2024 16:02","Lader 1","S1","SN1","2","0","Free session","RFID","EVDisconnected"
        """);

        // Act
        var result = await _service.ParseAsync(file);

        // Assert
        Assert.Single(result);

        var row = result[0];

        Assert.Equal("S1", row.SessionId);
        Assert.Equal("Lader 1", row.ChargerAlias);
        Assert.Equal(new DateTime(2024, 7, 30, 16, 2, 0), row.StartTime);
        Assert.Equal(new DateTime(2024, 7, 30, 18, 2, 0), row.EndTime);
        Assert.Equal(10.500m, row.EnergyKWh);
        Assert.Equal("Test Kunde", row.UserName);
    }

    [Fact]
    public async Task ParseAsync_SkipsEmptyLines()
    {
        // Arrange
        var file = CreateCsvFile("""
        "Session ID","Charger Alias","SN","Start Time","End Time","Duration(h)","Energy Delivered(kW·h)","Total Cost(kr)","Session Status","User Name","Authorization Type","Stop Reason"

        "S1","Lader 1","SN1","30/07/2024 16:02","30/07/2024 18:02","2","10.500","0","Free session","Test Kunde","RFID","EVDisconnected"

        """);

        // Act
        var result = await _service.ParseAsync(file);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task ParseAsync_SkipsLinesWithTooFewColumns()
    {
        // Arrange
        var file = CreateCsvFile("""
        "Session ID","Charger Alias","SN","Start Time","End Time","Duration(h)","Energy Delivered(kW·h)","Total Cost(kr)","Session Status","User Name","Authorization Type","Stop Reason"
        "S1","Lader 1"
        """);

        // Act
        var result = await _service.ParseAsync(file);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(';')]
    [InlineData('\t')]
    [InlineData(',')]
    public async Task ParseAsync_ReturnsRow_WhenCsvUsesSupportedDelimiter(char delimiter)
    {
        // Arrange
        var header = string.Join(delimiter, new[]
        {
            "Session ID", "Charger Alias", "SN", "Start Time", "End Time", "Duration(h)",
            "Energy Delivered(kW·h)", "Total Cost(kr)", "Session Status", "User Name",
            "Authorization Type", "Stop Reason"
        });

        var row = string.Join(delimiter, new[]
        {
            "S1", "Lader 1", "SN1", "30/07/2024 16:02", "30/07/2024 18:02", "2",
            "10.500", "0", "Free session", "Test Kunde", "RFID", "EVDisconnected"
        });

        var file = CreateCsvFile(header + Environment.NewLine + row);

        // Act
        var result = await _service.ParseAsync(file);

        // Assert
        Assert.Single(result);
        Assert.Equal("S1", result[0].SessionId);
        Assert.Equal("Test Kunde", result[0].UserName);
    }

    private static IFormFile CreateCsvFile(string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "file", "test.csv");
    }
}