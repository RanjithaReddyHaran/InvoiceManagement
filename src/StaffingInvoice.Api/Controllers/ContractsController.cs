using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.DTOs;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Controllers;

[ApiController, Route("api/contracts")]
public class ContractsController(InvoiceDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateContractRequest request, CancellationToken ct)
    {
        if (request.HourlyRate <= 0) return BadRequest(new { message = "Hourly rate must be greater than zero." });
        if (request.EndDate.HasValue && request.EndDate < request.StartDate) return BadRequest(new { message = "Contract end date cannot precede start date." });
        if (!await db.Resources.AnyAsync(x => x.Id == request.ResourceId && x.IsActive, ct)) return BadRequest(new { message = "Active resource was not found." });
        var contract = new Contract { ResourceId = request.ResourceId, HourlyRate = request.HourlyRate, Currency = request.Currency.ToUpperInvariant(), StartDate = request.StartDate, EndDate = request.EndDate };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync(ct);
        return Created($"/api/contracts/{contract.Id}", contract);
    }
}
