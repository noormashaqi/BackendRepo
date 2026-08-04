namespace SupermarketSystem.Api.Features.Returns.PureReturn;

public record PureReturnRequestBody(int ProductId, int QuantityReturned, long EmployeeId, string? Reason);