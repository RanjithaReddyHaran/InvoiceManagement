using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.DTOs;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Controllers;

[ApiController, Route("api/resources")]
public class ResourcesController(InvoiceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await db.Resources.Include(x => x.Client).AsNoTracking().ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateResourceRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Resource name and email are required." });
        if (!await db.Clients.AnyAsync(x => x.Id == request.ClientId && x.IsActive, ct)) return BadRequest(new { message = "Active client was not found." });
        var resource = new Resource { FullName = request.FullName.Trim(), Email = request.Email.Trim(), ClientId = request.ClientId };
        db.Resources.Add(resource);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = resource.Id }, resource);
    }
}
