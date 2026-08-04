namespace SupermarketSystem.Api.Models;

public class Return
{
    public long Id { get; init; }
    public long OriginalInvoiceId { get; init; }
    public string Type { get; init; } = string.Empty; // "Exchange" or "PureReturn"
    public int ProductId { get; init; }
    public int QuantityReturned { get; init; }
    public long? NewInvoiceId { get; init; }
    public long EmployeeId { get; init; }
    public DateTime Date { get; init; }
    public string? Reason { get; init; }
}