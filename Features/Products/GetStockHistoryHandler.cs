using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class GetStockHistoryHandler : IRequestHandler<GetStockHistoryQuery, List<StockHistoryDto>>
{
    private readonly IDbConnectionFactory _dbFactory;

    public GetStockHistoryHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<StockHistoryDto>> Handle(GetStockHistoryQuery request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var history = await connection.QueryAsync<StockHistoryDto>(@"
            SELECT sh.QuantityAdded, e.FullName AS EmployeeName, sh.Date
            FROM StockHistory sh
            INNER JOIN Employees e ON e.Id = sh.EmployeeId
            WHERE sh.ProductId = @ProductId
            ORDER BY sh.Date DESC",
            new { request.ProductId });

        return history.ToList();
    }
}