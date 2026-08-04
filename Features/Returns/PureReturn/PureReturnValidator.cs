using FluentValidation;

namespace SupermarketSystem.Api.Features.Returns.PureReturn;

public class PureReturnValidator : AbstractValidator<PureReturnCommand>
{
    public PureReturnValidator()
    {
        RuleFor(x => x.OriginalInvoiceId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.QuantityReturned).GreaterThan(0);
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}