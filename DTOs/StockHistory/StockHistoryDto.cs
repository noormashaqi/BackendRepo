namespace SupermarketSystem.Api.DTOs;

public class StockHistoryDto
{
    public int QuantityAdded { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}