using Ladestander.Api.Common;
using Ladestander.Api.DTOs.ChargingSessions;
using Ladestander.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ladestander.Api.Controllers;

[ApiController]
[Route("api/charging-sessions")]
[Authorize(Roles = Roles.Admin)]
public class ChargingSessionsController : ControllerBase
{
    private readonly IChargingSessionService _chargingSessionService;

    public ChargingSessionsController(IChargingSessionService chargingSessionService)
    {
        _chargingSessionService = chargingSessionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ChargingSessionResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await _chargingSessionService.GetAllAsync();

        return Ok(sessions);
    }

    [HttpGet("by-customer-and-period")]
    [ProducesResponseType(typeof(List<ChargingSessionResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> GetByCustomerAndPeriod(
    [FromQuery] int customerId,
    [FromQuery] int billingPeriodId)
    {
        if (customerId <= 0 || billingPeriodId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "CustomerId and BillingPeriodId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var sessions = await _chargingSessionService
            .GetByCustomerAndPeriodAsync(customerId, billingPeriodId);

        return Ok(sessions);
    }

    [HttpGet("{chargingSessionId}")]
    [ProducesResponseType(typeof(ChargingSessionResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetById(int chargingSessionId)
    {
        if (chargingSessionId <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = "ChargingSessionId must be greater than 0.",
                Timestamp = DateTime.UtcNow
            });
        }

        var session = await _chargingSessionService.GetByIdAsync(chargingSessionId);

        if (session is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"ChargingSession with id {chargingSessionId} was not found.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(session);
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(ChargingSessionResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> Import(ChargingSessionImportRequestDto request)
    {
        ChargingSessionResponseDto created;

        try
        {
            created = await _chargingSessionService.ImportAsync(request);
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

        return CreatedAtAction(
            nameof(GetById),
            new { chargingSessionId = created.ChargingSessionId },
            created
        );
    }
}