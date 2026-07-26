using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IDbConnectionFactory _dbFactory;

    public CreateProductHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var newId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO Product (Name, CategoryId, SellingPrice, Quantity, Unit, IsActive, CreatedAt)
            VALUES (@Name, @CategoryId, @SellingPrice, @Quantity, @Unit, 1, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();",
            new
            {
                request.Name,
                request.CategoryId,
                request.SellingPrice,
                request.Quantity,
                request.Unit
            });

        var categoryName = await connection.QuerySingleAsync<string>(
            "SELECT Name FROM Category WHERE Id = @CategoryId",
            new { request.CategoryId });

        return new ProductDto
        {
            Id = newId,
            Name = request.Name,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            SellingPrice = request.SellingPrice,
            Quantity = request.Quantity,
            Unit = request.Unit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}