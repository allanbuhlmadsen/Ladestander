using Ladestander.Api.Common;
using Ladestander.Api.DTOs.BillingPeriods;
using Ladestander.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ladestander.Api.Controllers;

[ApiController]
[Route("api/billing-periods")]
[Authorize(Roles = Roles.Admin)]
public class BillingPeriodsController : ControllerBase
{
    private readonly IBillingPeriodService _billingPeriodService;

    public BillingPeriodsController(IBillingPeriodService billingPeriodService)
    {
        _billingPeriodService = billingPeriodService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<BillingPeriodResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var periods = await _billingPeriodService.GetAllAsync();

        return Ok(periods);
    }

    [HttpGet("{billingPeriodId}")]
    [ProducesResponseType(typeof(BillingPeriodResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetById(int billingPeriodId)
    {
        if (billingPeriodId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "BillingPeriodId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var period = await _billingPeriodService.GetByIdAsync(billingPeriodId);

        if (period is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"BillingPeriod with id {billingPeriodId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(period);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BillingPeriodResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> Create(CreateBillingPeriodRequestDto request)
    {
        var createdPeriod = await _billingPeriodService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { billingPeriodId = createdPeriod.BillingPeriodId },
            createdPeriod
        );
    }

    [HttpPut("{billingPeriodId}")]
    [ProducesResponseType(typeof(BillingPeriodResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> Update(int billingPeriodId, UpdateBillingPeriodRequestDto request)
    {
        if (billingPeriodId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "BillingPeriodId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        BillingPeriodResponseDto? updatedPeriod;

        try
        {
            updatedPeriod = await _billingPeriodService.UpdateAsync(billingPeriodId, request);
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

        if (updatedPeriod is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"BillingPeriod with id {billingPeriodId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(updatedPeriod);
    }

    [HttpPatch("{billingPeriodId:int}/reopen")]
    [ProducesResponseType(typeof(BillingPeriodResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(int billingPeriodId)
    {
        if (billingPeriodId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "BillingPeriodId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var billingPeriod = await _billingPeriodService.ReopenAsync(billingPeriodId);

        if (billingPeriod is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"BillingPeriod with id {billingPeriodId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(billingPeriod);
    }
}