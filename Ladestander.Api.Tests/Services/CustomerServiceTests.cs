using Ladestander.Api.DTOs.Customers;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services;
using Moq;
using Xunit;

namespace Ladestander.Api.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _service = new CustomerService(_customerRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenRfidNumberAlreadyExists()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _customerRepositoryMock
            .Setup(repo => repo.ExistsByRfidNumberAsync(request.RfidNumber))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(request));

        // Assert
        Assert.Equal(
            $"Customer with RFID number {request.RfidNumber} already exists.",
            exception.Message);
    }

    [Fact]
    public async Task CreateAsync_CreatesCustomer_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _customerRepositoryMock
            .Setup(repo => repo.ExistsByRfidNumberAsync(request.RfidNumber))
            .ReturnsAsync(false);

        _customerRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer customer) =>
            {
                customer.CustomerId = 100;
                return customer;
            });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.Equal(100, result.CustomerId);
        Assert.Equal(request.RfidNumber, result.RfidNumber);
        Assert.Equal("Test Kunde", result.FullName);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Street, result.Street);
        Assert.Equal(request.HouseNumber, result.HouseNumber);
        Assert.Equal(request.PostalCode, result.PostalCode);
        Assert.Equal(request.City, result.City);
        Assert.True(result.IsActive);

        _customerRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<Customer>(customer =>
                customer.RfidNumber == request.RfidNumber &&
                customer.FirstName == request.FirstName &&
                customer.MiddleName == request.MiddleName &&
                customer.LastName == request.LastName &&
                customer.FullName == "Test Kunde" &&
                customer.IsActive == true)),
            Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 9999;

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.SoftDeleteAsync(customerId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsCustomerWithIsActiveFalse_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;

        var customer = new Customer
        {
            CustomerId = customerId,
            RfidNumber = "NO.TEST000001",
            FullName = "Test Kunde",
            Email = "testkunde@example.com",
            Street = "Testvej",
            HouseNumber = "1",
            PostalCode = 7000,
            City = "Fredericia",
            IsActive = true
        };

        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(repo => repo.SoftDeleteAsync(customer))
            .ReturnsAsync(() =>
            {
                customer.IsActive = false;
                return customer;
            });

        // Act
        var result = await _service.SoftDeleteAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result!.IsActive);

        _customerRepositoryMock.Verify(
            repo => repo.SoftDeleteAsync(customer),
            Times.Once);
    }

    private static CreateCustomerRequestDto CreateValidCreateRequest()
    {
        return new CreateCustomerRequestDto(
            RfidNumber: "NO.TEST000001",
            FirstName: "Test",
            MiddleName: null,
            LastName: "Kunde",
            Email: "testkunde@example.com",
            Street: "Testvej",
            HouseNumber: "1",
            PostalCode: 7000,
            City: "Fredericia"
        );
    }
}