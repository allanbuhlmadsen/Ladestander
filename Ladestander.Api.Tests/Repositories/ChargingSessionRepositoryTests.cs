using Ladestander.Api.Data;
using Ladestander.Api.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Tests.Repositories;

public class ChargingSessionRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task GetByCustomerAndPeriodAsync_ReturnsOnlyMatchingChargingSessions()
    {
        // Arrange
        using var context = CreateContext();

        context.Customers.AddRange(
            new Ladestander.Api.Entities.Customer
            {
                CustomerId = 1,
                RfidNumber = "NO.TEST000001",
                FirstName = "Test",
                LastName = "Kunde1",
                FullName = "Test Kunde1",
                IsActive = true
            },
            new Ladestander.Api.Entities.Customer
            {
                CustomerId = 2,
                RfidNumber = "NO.TEST000002",
                FirstName = "Test",
                LastName = "Kunde2",
                FullName = "Test Kunde2",
                IsActive = true
            });

        context.BillingPeriods.AddRange(
            new Ladestander.Api.Entities.BillingPeriod
            {
                BillingPeriodId = 1,
                Name = "Januar 2026",
                MonthName = "Januar"
            },
            new Ladestander.Api.Entities.BillingPeriod
            {
                BillingPeriodId = 2,
                Name = "Februar 2026",
                MonthName = "Februar"
            });

        context.ChargingSessions.AddRange(
            new Ladestander.Api.Entities.ChargingSession
            {
                CustomerId = 1,
                BillingPeriodId = 1,
                EnergyKWh = 10
            },
            new Ladestander.Api.Entities.ChargingSession
            {
                CustomerId = 1,
                BillingPeriodId = 1,
                EnergyKWh = 20
            },
            new Ladestander.Api.Entities.ChargingSession
            {
                CustomerId = 2,
                BillingPeriodId = 1,
                EnergyKWh = 30
            },
            new Ladestander.Api.Entities.ChargingSession
            {
                CustomerId = 1,
                BillingPeriodId = 2,
                EnergyKWh = 40
            });

        await context.SaveChangesAsync();

        var repository = new ChargingSessionRepository(context);

        // Act
        var result = await repository.GetByCustomerAndPeriodAsync(1, 1);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(result, session =>
        {
            Assert.Equal(1, session.CustomerId);
            Assert.Equal(1, session.BillingPeriodId);
        });
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenDuplicateChargingSessionExists()
    {
        // Arrange
        using var context = CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            CustomerId = 1,
            RfidNumber = "NO.TEST000003",
            FirstName = "Test",
            LastName = "Kunde",
            FullName = "Test Kunde",
            IsActive = true
        });

        context.BillingPeriods.Add(new Ladestander.Api.Entities.BillingPeriod
        {
            BillingPeriodId = 1,
            Name = "Januar 2026",
            MonthName = "Januar"
        });

        var startTime = new DateTime(2024, 7, 30, 16, 2, 0);

        context.ChargingSessions.Add(new Ladestander.Api.Entities.ChargingSession
        {
            CustomerId = 1,
            BillingPeriodId = 1,
            StartTime = startTime,
            ChargerAlias = "Lader 1",
            EnergyKWh = 10
        });

        await context.SaveChangesAsync();

        var repository = new ChargingSessionRepository(context);

        // Act
        var result = await repository.ExistsAsync(
            1,
            1,
            startTime,
            "Lader 1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenDuplicateChargingSessionDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();

        var repository = new ChargingSessionRepository(context);

        var startTime = new DateTime(2024, 7, 30, 16, 2, 0);

        // Act
        var result = await repository.ExistsAsync(
            1,
            1,
            startTime,
            "Lader 1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateRangeAsync_UpdatesInvoiceIdForMultipleChargingSessions()
    {
        // Arrange
        using var context = CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            CustomerId = 1,
            RfidNumber = "NO.TEST000004",
            FirstName = "Test",
            LastName = "Kunde",
            FullName = "Test Kunde",
            IsActive = true
        });

        context.BillingPeriods.Add(new Ladestander.Api.Entities.BillingPeriod
        {
            BillingPeriodId = 1,
            Name = "Januar 2026",
            MonthName = "Januar"
        });

        context.Invoices.Add(new Ladestander.Api.Entities.Invoice
        {
            InvoiceId = 1,
            CustomerId = 1,
            BillingPeriodId = 1,
            Status = "Draft",
            TotalEnergyKWh = 30,
            TotalAmount = 75
        });

        var sessions = new List<Ladestander.Api.Entities.ChargingSession>
        {
            new()
            {
                CustomerId = 1,
                BillingPeriodId = 1,
                EnergyKWh = 10
            },
            new()
            {
                CustomerId = 1,
                BillingPeriodId = 1,
                EnergyKWh = 20
            }
        };

        context.ChargingSessions.AddRange(sessions);

        await context.SaveChangesAsync();

        sessions[0].InvoiceId = 1;
        sessions[1].InvoiceId = 1;

        var repository = new ChargingSessionRepository(context);

        // Act
        await repository.UpdateRangeAsync(sessions);

        // Assert
        var updatedSessions = await context.ChargingSessions
            .Where(s => s.CustomerId == 1 && s.BillingPeriodId == 1)
            .ToListAsync();

        Assert.Equal(2, updatedSessions.Count);
        Assert.All(updatedSessions, session =>
        {
            Assert.Equal(1, session.InvoiceId);
        });
    }
}