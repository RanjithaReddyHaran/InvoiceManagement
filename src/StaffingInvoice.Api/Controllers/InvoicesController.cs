using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.DTOs;
using StaffingInvoice.Api.Services;

namespace StaffingInvoice.Api.Controllers;

[ApiController, Route("api/invoices")]
public class InvoicesController(IInvoiceService service, IPdfInvoiceService pdf, InvoiceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var invoice = await service.GetAsync(id, ct);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvoiceRequest request, CancellationToken ct)
    {
        var invoice = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct) => Ok(await service.ApproveAsync(id, ct));

    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> Payment(int id, RecordPaymentRequest request, CancellationToken ct) =>
        Ok(await service.RecordPaymentAsync(id, request, ct));

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> Pdf(int id, CancellationToken ct)
    {
        var invoice = await db.Invoices.Include(x => x.Client).Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (invoice is null) return NotFound();
        var bytes = pdf.Generate(invoice);
        return File(bytes, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
    }
}
