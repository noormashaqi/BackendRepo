namespace SupermarketSystem.Api.DTOs.Reports;

public class ProductDetailReportDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal SellingPrice { get; set; }

    public int CurrentStock { get; set; }

    public string Unit { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int GrossQuantitySold { get; set; }

    public int QuantityReturned { get; set; }

    public int NetQuantitySold { get; set; }

    public decimal GrossRevenue { get; set; }

    public decimal ReturnedRevenue { get; set; }

    public decimal NetRevenue { get; set; }

    public int SalesRank { get; set; }

    public int TotalProductsCount { get; set; }
}
