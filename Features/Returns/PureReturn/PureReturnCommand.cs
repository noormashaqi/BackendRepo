using MediatR;

namespace SupermarketSystem.Api.Features.Returns.PureReturn;

public record PureReturnCommand(
    long OriginalInvoiceId,
    int ProductId,
    int QuantityReturned,
    long EmployeeId,
    string? Reason
) : IRequest<PureReturnResult>;

public record PureReturnResult(long ReturnId);