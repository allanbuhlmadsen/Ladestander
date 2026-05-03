using Ladestander.Api.Common;
using Ladestander.Api.DTOs.Customers;
using Ladestander.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ladestander.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = Roles.Admin)]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _customerService.GetAllAsync();

        return Ok(customers);
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(CustomerResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetById(int customerId)
    {
        if (customerId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "CustomerId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var customer = await _customerService.GetByIdAsync(customerId);

        if (customer is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"Customer with id {customerId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(customer);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> Create(CreateCustomerRequestDto request)
    {
        try
        {
            var createdCustomer = await _customerService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { customerId = createdCustomer.CustomerId },
                createdCustomer
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPut("{customerId}")]
    [ProducesResponseType(typeof(CustomerResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> Update(int customerId, UpdateCustomerRequestDto request)
    {
        if (customerId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "CustomerId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var updatedCustomer = await _customerService.UpdateAsync(customerId, request);

        if (updatedCustomer is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"Customer with id {customerId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(updatedCustomer);
    }

    [HttpDelete("{customerId}")]
    [ProducesResponseType(typeof(CustomerResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> SoftDelete(int customerId)
    {
        if (customerId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "CustomerId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var deletedCustomer = await _customerService.SoftDeleteAsync(customerId);

        if (deletedCustomer is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"Customer with id {customerId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(deletedCustomer);
    }
}