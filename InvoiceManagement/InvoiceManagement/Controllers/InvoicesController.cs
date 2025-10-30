using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagement.Models.DTOs;
using InvoiceManagement.Models.Entities;
using InvoiceManagement.Services;

namespace InvoiceManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILockService _lockService;
    private readonly UserManager<User> _userManager;

    public InvoicesController(IInvoiceService invoiceService, ILockService lockService, UserManager<User> userManager)
    {
        _invoiceService = invoiceService;
        _lockService = lockService;
        _userManager = userManager;
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Accountant,Administrator")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceResponse>> CreateInvoice(CreateInvoiceRequest request)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var invoice = await _invoiceService.CreateAsync(request, user.Id);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> GetInvoice(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            return NotFound(new { error = $"Invoice with ID {id} not found." });
        }

        return Ok(invoice);
    }

    /// <summary>
    /// List and filter invoices with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(InvoiceListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InvoiceListResponse>> ListInvoices([FromQuery] InvoiceFilterRequest filter)
    {
        var result = await _invoiceService.GetFilteredAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing invoice
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Accountant,Administrator")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LockResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceResponse>> UpdateInvoice(int id, UpdateInvoiceRequest request)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Check if invoice is locked
            var lockStatus = await _lockService.IsLockedAsync(id);
            if (lockStatus != null && lockStatus.LockedByUserId != user.Id)
            {
                return Conflict(lockStatus);
            }

            var invoice = await _invoiceService.UpdateAsync(id, request, user.Id);

            // Release lock after successful update
            await _lockService.ReleaseLockAsync(id, user.Id);

            return Ok(invoice);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Acquire a lock on an invoice for editing
    /// </summary>
    [HttpPost("{id}/lock")]
    [Authorize(Roles = "Accountant,Administrator")]
    [ProducesResponseType(typeof(LockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LockResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LockResponse>> AcquireLock(int id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var lockResponse = await _lockService.AcquireLockAsync(id, user.Id, user.UserName ?? "Unknown");

            // If lock is held by another user, return 409 Conflict
            if (lockResponse.LockedByUserId != user.Id)
            {
                return Conflict(lockResponse);
            }

            return Ok(lockResponse);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Release a lock on an invoice
    /// </summary>
    [HttpDelete("{id}/lock")]
    [Authorize(Roles = "Accountant,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseLock(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var result = await _lockService.ReleaseLockAsync(id, user.Id);

        if (!result)
        {
            return NotFound(new { error = "No lock found or you don't own the lock." });
        }

        return NoContent();
    }

    /// <summary>
    /// Export invoices to CSV format
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportInvoices([FromQuery] InvoiceFilterRequest filter)
    {
        try
        {
            var stream = await _invoiceService.ExportToCsvAsync(filter);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"invoices_{timestamp}.csv";

            return File(stream, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
