using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Reports;

public record GetInventoryReportQuery(
    int? CategoryId,
    bool ActiveOnly
) : IRequest<InventoryReportDto>;

public class GetInventoryReportHandler : IRequestHandler<GetInventoryReportQuery, InventoryReportDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetInventoryReportHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<InventoryReportDto> Handle(
        GetInventoryReportQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                p.Id AS ProductId,
                p.Name AS ProductName,
                p.CategoryId,
                c.Name AS CategoryName,
                p.SellingPrice,
                p.Quantity,
                p.Unit,
                p.IsActive,
                p.CreatedAt
            FROM Product p
            INNER JOIN Category c
                ON c.Id = p.CategoryId
            WHERE (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
              AND (@ActiveOnly = FALSE OR p.IsActive = TRUE)
            ORDER BY c.Name, p.Name;
            """;

        var rows = (await connection.QueryAsync<InventoryReportItemDto>(
            new CommandDefinition(
                sql,
                new
                {
                    request.CategoryId,
                    request.ActiveOnly
                },
                cancellationToken: cancellationToken))).ToList();

        return new InventoryReportDto
        {
            CategoryId = request.CategoryId,
            ActiveOnly = request.ActiveOnly,
            ProductCount = rows.Count,
            TotalQuantity = rows.Sum(x => x.Quantity),
            TotalEstimatedSalesValue = rows.Sum(x => x.SellingPrice * x.Quantity),
            Products = rows
        };
    }
}
