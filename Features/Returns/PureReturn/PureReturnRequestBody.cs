namespace SupermarketSystem.Api.Features.Returns.PureReturn;

public record PureReturnRequestBody(
    int ProductId,
    int QuantityReturned,
    string? Reason
);