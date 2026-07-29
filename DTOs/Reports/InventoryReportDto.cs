namespace SupermarketSystem.Api.DTOs.Reports;

public class InventoryReportDto
{
    public int? CategoryId { get; set; }

    public bool ActiveOnly { get; set; }

    public int ProductCount { get; set; }

    public int TotalQuantity { get; set; }

    public decimal TotalEstimatedSalesValue { get; set; }

    public IReadOnlyCollection<InventoryReportItemDto> Products { get; set; } = Array.Empty<InventoryReportItemDto>();
}

public class InventoryReportItemDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal SellingPrice { get; set; }

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
