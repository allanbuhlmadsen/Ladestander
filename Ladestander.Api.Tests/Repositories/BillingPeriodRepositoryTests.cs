using Ladestander.Api.Repositories;
using Ladestander.Api.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Tests.Repositories;

public class BillingPeriodRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_SavesIsClosedCorrectly()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

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