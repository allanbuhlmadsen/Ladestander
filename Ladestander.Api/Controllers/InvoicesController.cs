using Ladestander.Api.Common;
using Ladestander.Api.DTOs.Invoices;
using Ladestander.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ladestander.Api.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize(Roles = Roles.Admin)]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _invoiceService.GetAllAsync();

            return Ok(invoices);
        }

        [HttpGet("{invoiceId:int}")]
        [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int invoiceId)
        {
            if (invoiceId <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "InvoiceId must be greater than 0.",
                    Timestamp = DateTime.UtcNow
                });
            }

            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            if (invoice is null)
            {
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Invoice with id {invoiceId} was not found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(invoice);
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Generate(GenerateInvoiceRequestDto request)
        {
            try
            {
                var invoice = await _invoiceService.GenerateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { invoiceId = invoice.InvoiceId },
                    invoice
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

        [HttpPatch("{invoiceId:int}/status")]
        [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int invoiceId, UpdateInvoiceStatusRequestDto request)
        {
            if (invoiceId <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "InvoiceId must be greater than 0.",
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var invoice = await _invoiceService.UpdateStatusAsync(invoiceId, request);

                if (invoice is null)
                {
                    return NotFound(new ErrorResponse
                    {
                        StatusCode = 404,
                        Message = $"Invoice with id {invoiceId} was not found.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(invoice);
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

        [HttpGet("by-customer-and-period")]
        [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCustomerAndPeriod(int customerId, int billingPeriodId)
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

            var invoice = await _invoiceService.GetByCustomerAndPeriodAsync(customerId, billingPeriodId);

            if (invoice is null)
            {
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Invoice for CustomerId {customerId} and BillingPeriodId {billingPeriodId} was not found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(invoice);
        }
    }
}