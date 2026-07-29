using Dapper;
using FluentValidation;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class AddStockValidator : AbstractValidator<AddStockCommand>
{
    private readonly IDbConnectionFactory _dbFactory;

    public AddStockValidator(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;

        RuleFor(x => x.QuantityAdded)
            .GreaterThan(0).WithMessage("QuantityAdded must be greater than 0");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId is required");

        RuleFor(x => x.ProductId)
            .MustAsync(ProductExists).WithMessage("Product does not exist");
    }

    private async Task<bool> ProductExists(int productId, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var exists = await connection.QuerySingleOrDefaultAsync<int?>(
            "SELECT Id FROM Product WHERE Id = @ProductId",
            new { ProductId = productId });

        return exists is not null;
    }
}