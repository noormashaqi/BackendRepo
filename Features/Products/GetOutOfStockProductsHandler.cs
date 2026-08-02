using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class GetOutOfStockProductsHandler : IRequestHandler<GetOutOfStockProductsQuery, List<ProductDto>>
{
    private readonly IDbConnectionFactory _dbFactory;

    public GetOutOfStockProductsHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ProductDto>> Handle(GetOutOfStockProductsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var products = await connection.QueryAsync<ProductDto>(@"
            SELECT p.Id, p.Name, p.CategoryId, c.Name AS CategoryName,
                   p.SellingPrice, p.Quantity, p.Unit, p.IsActive, p.CreatedAt
            FROM Product p
            INNER JOIN Category c ON c.Id = p.CategoryId
            WHERE p.Quantity = 0 AND p.IsActive = 1
            ORDER BY p.Name");

        return products.ToList();
    }
}