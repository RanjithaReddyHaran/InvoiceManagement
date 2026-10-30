namespace StaffingInvoice.Api.DTOs;

public record CreateClientRequest(string Name, string BillingEmail, string BillingAddress, int PaymentTermsDays = 30);
public record CreateResourceRequest(string FullName, string Email, int ClientId);
public record CreateContractRequest(int ResourceId, decimal HourlyRate, string Currency, DateOnly StartDate, DateOnly? EndDate);
public record CreateTimesheetRequest(int ResourceId, DateOnly PeriodStart, DateOnly PeriodEnd, decimal RegularHours, decimal OvertimeHours);
public record CreateInvoiceRequest(int ClientId, DateOnly PeriodStart, DateOnly PeriodEnd, decimal TaxRate = 0, string? Notes = null);
public record RecordPaymentRequest(decimal Amount, DateOnly PaymentDate, string Method, string? Reference);
public record InvoiceSummaryDto(int Id, string InvoiceNumber, string ClientName, DateOnly InvoiceDate, DateOnly DueDate, decimal TotalAmount, decimal AmountPaid, decimal BalanceDue, string Status);
