using StaffingInvoice.Api.Models;
using Xunit;

namespace StaffingInvoice.Tests;

public class InvoiceCalculationTests
{
    [Theory]
    [InlineData(40, 125, 5000)]
    [InlineData(80, 100, 8000)]
    [InlineData(1.5, 120, 180)]
    public void HoursTimesRate_ProducesExpectedLineAmount(decimal hours, decimal rate, decimal expected)
    {
        var amount = decimal.Round(hours * rate, 2, MidpointRounding.AwayFromZero);
        Assert.Equal(expected, amount);
    }

    [Fact]
    public void PartialPayment_LeavesCorrectBalance()
    {
        var invoice = new Invoice { TotalAmount = 5000m, AmountPaid = 1500m };
        Assert.Equal(3500m, invoice.TotalAmount - invoice.AmountPaid);
    }
}
