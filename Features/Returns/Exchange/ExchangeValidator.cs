using FluentValidation;

namespace SupermarketSystem.Api.Features.Returns.Exchange;

public class ExchangeValidator : AbstractValidator<ExchangeCommand>
{
    public ExchangeValidator()
    {
        RuleFor(x => x.OriginalInvoiceId).GreaterThan(0);
        RuleFor(x => x.OldProductId).GreaterThan(0);
        RuleFor(x => x.QuantityReturned).GreaterThan(0);
        RuleFor(x => x.NewProductId).GreaterThan(0);
        RuleFor(x => x.NewQuantity).GreaterThan(0);
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}