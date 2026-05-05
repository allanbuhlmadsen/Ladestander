using Ladestander.Api.Repositories;
using Ladestander.Api.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Tests.Repositories;

public class CustomerRepositoryTests
{
    [Fact]
    public async Task GetByFullNameAsync_ReturnsCustomer_WhenFullNameIncludesMiddleName()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            RfidNumber = "NO.TEST000001",
            FirstName = "Hans",
            MiddleName = "Peter",
            LastName = "Jensen",
            FullName = "Hans Peter Jensen",
            IsActive = true
        });

        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByFullNameAsync("Hans Peter Jensen");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hans", result.FirstName);
        Assert.Equal("Peter", result.MiddleName);
        Assert.Equal("Jensen", result.LastName);
    }

    [Fact]
    public async Task GetByFullNameAsync_ReturnsCustomer_WhenUsingFallbackWithoutMiddleName()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            RfidNumber = "NO.TEST000002",
            FirstName = "Hans",
            MiddleName = "Peter",
            LastName = "Jensen",
            FullName = "Hans Peter Jensen",
            IsActive = true
        });

        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByFullNameAsync("Hans Jensen");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hans", result.FirstName);
        Assert.Equal("Peter", result.MiddleName);
        Assert.Equal("Jensen", result.LastName);
    }

    [Fact]
    public async Task GetByFullNameAsync_ReturnsCustomer_WhenNameUsesDifferentCasing()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            RfidNumber = "NO.TEST000003",
            FirstName = "Hans",
            MiddleName = "Peter",
            LastName = "Jensen",
            FullName = "Hans Peter Jensen",
            IsActive = true
        });

        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByFullNameAsync("hAnS pEtEr jEnSeN");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hans", result.FirstName);
        Assert.Equal("Peter", result.MiddleName);
        Assert.Equal("Jensen", result.LastName);
    }

    [Fact]
    public async Task GetByFullNameAsync_ReturnsCustomer_WhenNameContainsExtraSpaces()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            RfidNumber = "NO.TEST000004",
            FirstName = "Hans",
            MiddleName = "Peter",
            LastName = "Jensen",
            FullName = "Hans Peter Jensen",
            IsActive = true
        });

        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByFullNameAsync("  Hans   Peter   Jensen  ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hans", result.FirstName);
        Assert.Equal("Peter", result.MiddleName);
        Assert.Equal("Jensen", result.LastName);
    }

    [Fact]
    public async Task ExistsByRfidNumberAsync_ReturnsTrue_WhenRfidNumberExists()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

        context.Customers.Add(new Ladestander.Api.Entities.Customer
        {
            RfidNumber = "NO.TEST999999",
            FirstName = "Test",
            LastName = "Kunde",
            FullName = "Test Kunde",
            IsActive = true
        });

        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.ExistsByRfidNumberAsync("NO.TEST999999");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByRfidNumberAsync_ReturnsFalse_WhenRfidNumberDoesNotExist()
    {
        // Arrange
        using var context = SqliteTestDbContextFactory.CreateContext();

        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.ExistsByRfidNumberAsync("NO.UNKNOWN");

        // Assert
        Assert.False(result);
    }
}