namespace SupermarketSystem.Api.Features.Returns.Exchange;

public record ExchangeRequestBody(
    int OldProductId,
    int QuantityReturned,
    int NewProductId,
    int NewQuantity,
    long EmployeeId,
    string? Reason
);