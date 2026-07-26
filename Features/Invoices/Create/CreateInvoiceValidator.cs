using FluentValidation;

namespace SupermarketSystem.Api.Features.Invoices.Create;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Invoice must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}