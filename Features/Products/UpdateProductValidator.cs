using Dapper;
using FluentValidation;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    private readonly IDbConnectionFactory _dbFactory;
    private static readonly string[] ValidUnits = { "Piece", "Package" };

    public UpdateProductValidator(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters");

        RuleFor(x => x.SellingPrice)
            .GreaterThan(0).WithMessage("Selling price must be greater than 0");

        RuleFor(x => x.Unit)
            .Must(u => ValidUnits.Contains(u))
            .WithMessage("Unit must be either 'Piece' or 'Package'");

        RuleFor(x => x.CategoryId)
            .MustAsync(CategoryExists).WithMessage("CategoryId does not exist");

        RuleFor(x => x.Id)
            .MustAsync(ProductExists).WithMessage("Product does not exist");
    }

    private async Task<bool> CategoryExists(int categoryId, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var exists = await connection.QuerySingleOrDefaultAsync<int?>(
            "SELECT Id FROM Category WHERE Id = @CategoryId",
            new { CategoryId = categoryId });

        return exists is not null;
    }

    private async Task<bool> ProductExists(int id, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var exists = await connection.QuerySingleOrDefaultAsync<int?>(
            "SELECT Id FROM Product WHERE Id = @Id",
            new { Id = id });

        return exists is not null;
    }
}