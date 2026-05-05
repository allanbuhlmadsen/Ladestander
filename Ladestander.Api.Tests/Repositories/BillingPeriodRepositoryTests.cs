using Ladestander.Api.Data;
using Ladestander.Api.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Tests.Repositories;

public class BillingPeriodRepositoryTests
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
    public async Task UpdateAsync_SavesIsClosedCorrectly()
    {
        // Arrange
        using var context = CreateContext();

        var billingPeriod = new Ladestander.Api.Entities.BillingPeriod
        {
            Name = "Januar 2026",
            MonthName = "Januar",
            IsClosed = false
        };

        context.BillingPeriods.Add(billingPeriod);
        await context.SaveChangesAsync();

        billingPeriod.IsClosed = true;

        var repository = new BillingPeriodRepository(context);

        // Act
        var result = await repository.UpdateAsync(billingPeriod);

        // Assert
        Assert.True(result.IsClosed);

        var savedBillingPeriod = await context.BillingPeriods
            .FirstOrDefaultAsync(p => p.BillingPeriodId == billingPeriod.BillingPeriodId);

        Assert.NotNull(savedBillingPeriod);
        Assert.True(savedBillingPeriod.IsClosed);
    }
}