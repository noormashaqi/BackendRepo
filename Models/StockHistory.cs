namespace SupermarketSystem.Api.Models;

public class StockHistory
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int QuantityAdded { get; set; }
    public long EmployeeId { get; set; }
    public DateTime Date { get; set; }
}