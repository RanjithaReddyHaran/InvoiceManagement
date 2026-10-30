using Microsoft.EntityFrameworkCore;
using StaffingInvoice.Api.Data;
using StaffingInvoice.Api.DTOs;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Services;

public interface IInvoiceService
{
    Task<Invoice> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken);
    Task<Invoice?> GetAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvoiceSummaryDto>> ListAsync(CancellationToken cancellationToken);
    Task<Invoice> ApproveAsync(int id, CancellationToken cancellationToken);
    Task<Invoice> RecordPaymentAsync(int id, RecordPaymentRequest request, CancellationToken cancellationToken);
}

public class InvoiceService(InvoiceDbContext db, ILogger<InvoiceService> logger) : IInvoiceService
{
    public async Task<Invoice> CreateAsync(CreateInvoiceRequest request, CancellationToken ct)
    {
        if (request.PeriodEnd < request.PeriodStart) throw new ArgumentException("Period end must be on or after period start.");
        if (request.TaxRate is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(request.TaxRate), "Tax rate must be between 0 and 100.");

        var client = await db.Clients.SingleOrDefaultAsync(x => x.Id == request.ClientId && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Active client was not found.");

        // Only approved, uninvoiced timesheets belonging to the selected client and period are billable.
        var sheets = await db.Timesheets.Include(x => x.Resource)
            .Where(x => x.Resource!.ClientId == request.ClientId && x.Status == "Approved" &&
                        x.InvoiceId == null && x.PeriodStart >= request.PeriodStart && x.PeriodEnd <= request.PeriodEnd)
            .OrderBy(x => x.Resource!.FullName).ToListAsync(ct);

        if (sheets.Count == 0) throw new InvalidOperationException("No approved, uninvoiced timesheets were found for this client and period.");

        var invoiceDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            ClientId = client.Id,
            InvoiceDate = invoiceDate,
            DueDate = invoiceDate.AddDays(client.PaymentTermsDays),
            TaxRate = request.TaxRate,
            Notes = request.Notes
        };

        foreach (var sheet in sheets)
        {
            var contract = await db.Contracts
                .Where(x => x.ResourceId == sheet.ResourceId && x.IsActive &&
                            x.StartDate <= sheet.PeriodEnd && (x.EndDate == null || x.EndDate >= sheet.PeriodStart))
                .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException($"No active contract/rate found for resource '{sheet.Resource?.FullName}'.");

            if (contract.Currency != invoice.Currency)
                throw new InvalidOperationException("All invoice lines must use the same currency.");

            var hours = sheet.RegularHours + sheet.OvertimeHours;
            var amount = decimal.Round(hours * contract.HourlyRate, 2, MidpointRounding.AwayFromZero);
            invoice.Lines.Add(new InvoiceLine
            {
                TimesheetId = sheet.Id,
                ResourceName = sheet.Resource?.FullName ?? "Unknown resource",
                Description = $"Staffing services: {sheet.PeriodStart:MMM dd} - {sheet.PeriodEnd:MMM dd}",
                Hours = hours,
                HourlyRate = contract.HourlyRate,
                Amount = amount
            });
            invoice.Subtotal += amount;
        }

        invoice.Subtotal = decimal.Round(invoice.Subtotal, 2);
        invoice.TaxAmount = decimal.Round(invoice.Subtotal * invoice.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
        invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(ct);
        foreach (var sheet in sheets) sheet.InvoiceId = invoice.Id;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Created invoice {InvoiceNumber} for client {ClientId}", invoice.InvoiceNumber, client.Id);
        return invoice;
    }

    public Task<Invoice?> GetAsync(int id, CancellationToken ct) =>
        db.Invoices.Include(x => x.Client).Include(x => x.Lines).Include(x => x.Payments)
          .SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<InvoiceSummaryDto>> ListAsync(CancellationToken ct) =>
        await db.Invoices.AsNoTracking().Include(x => x.Client)
            .OrderByDescending(x => x.InvoiceDate)
            .Select(x => new InvoiceSummaryDto(x.Id, x.InvoiceNumber, x.Client!.Name, x.InvoiceDate, x.DueDate,
                x.TotalAmount, x.AmountPaid, x.TotalAmount - x.AmountPaid, x.Status)).ToListAsync(ct);

    public async Task<Invoice> ApproveAsync(int id, CancellationToken ct)
    {
        var invoice = await db.Invoices.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Invoice was not found.");
        if (invoice.Status != "Draft") throw new InvalidOperationException("Only draft invoices can be approved.");
        if (invoice.Lines.Count == 0) throw new InvalidOperationException("An invoice must contain at least one line.");
        invoice.Status = "Approved";
        await db.SaveChangesAsync(ct);
        return invoice;
    }

    public async Task<Invoice> RecordPaymentAsync(int id, RecordPaymentRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(request.Amount), "Payment amount must be greater than zero.");
        var invoice = await db.Invoices.Include(x => x.Payments).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Invoice was not found.");
        if (invoice.Status is "Draft" or "Void") throw new InvalidOperationException("Payments can only be recorded for approved or sent invoices.");
        var balance = invoice.TotalAmount - invoice.AmountPaid;
        if (request.Amount > balance) throw new InvalidOperationException("Payment exceeds the outstanding balance.");
        invoice.Payments.Add(new Payment { Amount = request.Amount, PaymentDate = request.PaymentDate, Method = request.Method, Reference = request.Reference });
        invoice.AmountPaid += request.Amount;
        if (invoice.AmountPaid == invoice.TotalAmount) invoice.Status = "Paid";
        await db.SaveChangesAsync(ct);
        return invoice;
    }
}
