using Ladestander.Api.Data;
using Ladestander.Api.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Tests.Repositories;

public class InvoiceRepositoryTests
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
    public async Task ExistsAsync_ReturnsTrue_WhenInvoiceExistsForCustomerAndBillingPeriod()
    {
        // Arrange
        using var context = CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            CustomerId = 1,
            RfidNumber = "NO.TEST000001",
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
            CustomerId = 1,
            BillingPeriodId = 1,
            Status = "Draft",
            TotalEnergyKWh = 100,
            TotalAmount = 250
        });

        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.ExistsAsync(1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenInvoiceDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.ExistsAsync(1, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByCustomerAndPeriodAsync_ReturnsCorrectInvoice()
    {
        // Arrange
        using var context = CreateContext();

        context.Customers.AddRange(
            new Ladestander.Api.Entities.Customer
            {
                CustomerId = 1,
                RfidNumber = "NO.TEST000002",
                FirstName = "Test",
                LastName = "Kunde1",
                FullName = "Test Kunde1",
                IsActive = true
            },
            new Ladestander.Api.Entities.Customer
            {
                CustomerId = 2,
                RfidNumber = "NO.TEST000003",
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

        context.Invoices.AddRange(
            new Ladestander.Api.Entities.Invoice
            {
                CustomerId = 1,
                BillingPeriodId = 1,
                Status = "Draft",
                TotalEnergyKWh = 10,
                TotalAmount = 25
            },
            new Ladestander.Api.Entities.Invoice
            {
                CustomerId = 2,
                BillingPeriodId = 1,
                Status = "Draft",
                TotalEnergyKWh = 20,
                TotalAmount = 50
            },
            new Ladestander.Api.Entities.Invoice
            {
                CustomerId = 1,
                BillingPeriodId = 2,
                Status = "Draft",
                TotalEnergyKWh = 30,
                TotalAmount = 75
            });

        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetByCustomerAndPeriodAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal(1, result.BillingPeriodId);
        Assert.Equal(10, result.TotalEnergyKWh);
        Assert.Equal(25, result.TotalAmount);
    }

    [Fact]
    public async Task AddAsync_SavesInvoiceCorrectly()
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

        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        var invoice = new Ladestander.Api.Entities.Invoice
        {
            CustomerId = 1,
            BillingPeriodId = 1,
            Status = "Draft",
            TotalEnergyKWh = 96.302m,
            TotalAmount = 205.44m
        };

        // Act
        var result = await repository.AddAsync(invoice);

        // Assert
        Assert.True(result.InvoiceId > 0);

        var savedInvoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceId == result.InvoiceId);

        Assert.NotNull(savedInvoice);
        Assert.Equal(1, savedInvoice.CustomerId);
        Assert.Equal(1, savedInvoice.BillingPeriodId);
        Assert.Equal("Draft", savedInvoice.Status);
        Assert.Equal(96.302m, savedInvoice.TotalEnergyKWh);
        Assert.Equal(205.44m, savedInvoice.TotalAmount);
    }
}