using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class GetLowStockProductsHandler : IRequestHandler<GetLowStockProductsQuery, List<ProductDto>>
{
    private const int LowStockThreshold = 10;

    private readonly IDbConnectionFactory _dbFactory;

    public GetLowStockProductsHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var products = await connection.QueryAsync<ProductDto>(@"
            SELECT p.Id, p.Name, p.CategoryId, c.Name AS CategoryName,
                   p.SellingPrice, p.Quantity, p.Unit, p.IsActive, p.CreatedAt
            FROM Product p
            INNER JOIN Category c ON c.Id = p.CategoryId
            WHERE p.Quantity <= @Threshold AND p.IsActive = 1
            ORDER BY p.Quantity ASC",
            new { Threshold = LowStockThreshold });

        return products.ToList();
    }
}