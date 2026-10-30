using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.DTOs;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Controllers;

[ApiController, Route("api/timesheets")]
public class TimesheetsController(InvoiceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await db.Timesheets.Include(x => x.Resource).AsNoTracking().ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTimesheetRequest request, CancellationToken ct)
    {
        if (request.PeriodEnd < request.PeriodStart || request.RegularHours < 0 || request.OvertimeHours < 0 ||
            request.RegularHours + request.OvertimeHours <= 0 || request.RegularHours + request.OvertimeHours > 744)
            return BadRequest(new { message = "Enter a valid period and positive hours (maximum 744 hours)." });
        if (!await db.Resources.AnyAsync(x => x.Id == request.ResourceId && x.IsActive, ct)) return BadRequest(new { message = "Active resource was not found." });
        var sheet = new Timesheet { ResourceId = request.ResourceId, PeriodStart = request.PeriodStart, PeriodEnd = request.PeriodEnd, RegularHours = request.RegularHours, OvertimeHours = request.OvertimeHours };
        db.Timesheets.Add(sheet);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = sheet.Id }, sheet);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var sheet = await db.Timesheets.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (sheet is null) return NotFound();
        if (sheet.InvoiceId is not null) return Conflict(new { message = "An invoiced timesheet cannot be changed." });
        if (sheet.Status != "Pending") return Conflict(new { message = "Only pending timesheets can be approved." });
        sheet.Status = "Approved"; sheet.ApprovedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(sheet);
    }
}
