namespace StaffingInvoice.Api.Models;

/// <summary>Represents a customer organization receiving staffing services.</summary>
public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BillingEmail { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public int PaymentTermsDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}

/// <summary>Represents a consultant supplied by the staffing company.</summary>
public class Resource
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Contractual billing terms for a resource/client engagement.</summary>
public class Contract
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }
    public decimal HourlyRate { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Approved billable time submitted for an engagement.</summary>
public class Timesheet
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? ApprovedUtc { get; set; }
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
}

/// <summary>Invoice header and immutable financial snapshot.</summary>
public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

/// <summary>Invoice line snapshots resource, hours, rate and amount at issue time.</summary>
public class InvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public int TimesheetId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>Records full or partial client payments against an invoice.</summary>
public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string Method { get; set; } = "ACH";
    public string? Reference { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
