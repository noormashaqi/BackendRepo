using Dapper;
using FluentValidation;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Categories;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly IDbConnectionFactory _dbFactory;

    public CreateCategoryValidator(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
            .MustAsync(BeUniqueName).WithMessage("Category name already exists");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var exists = await connection.QuerySingleOrDefaultAsync<int?>(
            "SELECT Id FROM Category WHERE Name = @Name",
            new { Name = name });

        return exists is null;
    }
}