using Microsoft.AspNetCore.Mvc;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.DTOs;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Controllers;

[ApiController, Route("api/clients")]
public class ClientsController(InvoiceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(db.Clients.AsNoTracking(), ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateClientRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.BillingEmail))
            return BadRequest(new { message = "Client name and billing email are required." });
        if (request.PaymentTermsDays is < 0 or > 180) return BadRequest(new { message = "Payment terms must be between 0 and 180 days." });
        var client = new Client { Name = request.Name.Trim(), BillingEmail = request.BillingEmail.Trim(), BillingAddress = request.BillingAddress.Trim(), PaymentTermsDays = request.PaymentTermsDays };
        db.Clients.Add(client);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
    }
}
