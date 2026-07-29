namespace SupermarketSystem.Api.DTOs.Reports;

public class SalesReportDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int? EmployeeId { get; set; }

    public int InvoiceCount { get; set; }

    public decimal TotalSalesBeforeDiscount { get; set; }

    public decimal TotalDiscountAmount { get; set; }

    public decimal TotalSalesAfterDiscount { get; set; }

    public IReadOnlyCollection<SalesReportInvoiceDto> Invoices { get; set; } = Array.Empty<SalesReportInvoiceDto>();
}

public class SalesReportInvoiceDto
{
    public long InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public long EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public decimal TotalBeforeDiscount { get; set; }

    public decimal DiscountPercentage { get; set; }

    public decimal TotalAfterDiscount { get; set; }

    public bool HasReturn { get; set; }
}
