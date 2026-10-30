using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Services;

public interface IPdfInvoiceService { byte[] Generate(Invoice invoice); }

public class PdfInvoiceService : IPdfInvoiceService
{
    public byte[] Generate(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(container => container.Page(page =>
        {
            page.Margin(35);
            page.Header().Column(col =>
            {
                col.Item().Text("STAFFING SERVICES INVOICE").FontSize(22).Bold();
                col.Item().Text(invoice.InvoiceNumber).FontSize(12).FontColor(Colors.Grey.Darken1);
            });
            page.Content().PaddingVertical(20).Column(col =>
            {
                col.Item().Text($"Bill To: {invoice.Client?.Name}").Bold();
                col.Item().Text(invoice.Client?.BillingAddress ?? string.Empty);
                col.Item().Text($"Invoice Date: {invoice.InvoiceDate:MMM dd, yyyy}    Due Date: {invoice.DueDate:MMM dd, yyyy}");
                col.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(4); columns.RelativeColumn(1); columns.RelativeColumn(1); columns.RelativeColumn(1); });
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Resource");
                        header.Cell().Element(CellStyle).Text("Description");
                        header.Cell().Element(CellStyle).AlignRight().Text("Hours");
                        header.Cell().Element(CellStyle).AlignRight().Text("Rate");
                        header.Cell().Element(CellStyle).AlignRight().Text("Amount");
                    });
                    foreach (var line in invoice.Lines)
                    {
                        table.Cell().Element(CellStyle).Text(line.ResourceName);
                        table.Cell().Element(CellStyle).Text(line.Description);
                        table.Cell().Element(CellStyle).AlignRight().Text(line.Hours.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(line.HourlyRate.ToString("C"));
                        table.Cell().Element(CellStyle).AlignRight().Text(line.Amount.ToString("C"));
                    }
                });
                col.Item().AlignRight().Text($"Subtotal: {invoice.Subtotal:C}");
                col.Item().AlignRight().Text($"Tax ({invoice.TaxRate:N2}%): {invoice.TaxAmount:C}");
                col.Item().AlignRight().Text($"Total: {invoice.TotalAmount:C}").FontSize(16).Bold();
                col.Item().AlignRight().Text($"Paid: {invoice.AmountPaid:C}");
                col.Item().AlignRight().Text($"Balance Due: {invoice.TotalAmount - invoice.AmountPaid:C}").Bold();
                if (!string.IsNullOrWhiteSpace(invoice.Notes)) col.Item().PaddingTop(15).Text($"Notes: {invoice.Notes}");
            });
            page.Footer().AlignCenter().Text("Thank you for your business.").FontSize(9).FontColor(Colors.Grey.Medium);
        })).GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingHorizontal(4);
}
