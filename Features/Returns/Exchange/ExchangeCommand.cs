using MediatR;

namespace SupermarketSystem.Api.Features.Returns.Exchange;

public record ExchangeCommand(
    long OriginalInvoiceId,
    int OldProductId,
    int QuantityReturned,
    int NewProductId,
    int NewQuantity,
    long EmployeeId,
    string? Reason
) : IRequest<ExchangeResult>;

public record ExchangeResult(long ReturnId, long NewInvoiceId, string NewInvoiceNumber);