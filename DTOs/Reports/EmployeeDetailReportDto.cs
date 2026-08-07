namespace SupermarketSystem.Api.DTOs.Reports;

public class EmployeeDetailReportDto
{
    public long EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int InvoiceCount { get; set; }

    public decimal GrossSales { get; set; }

    public decimal TotalReturnedAmount { get; set; }

    public decimal NetSales { get; set; }

    public IReadOnlyCollection<SalesReportInvoiceDto> Invoices { get; set; } = Array.Empty<SalesReportInvoiceDto>();
}
