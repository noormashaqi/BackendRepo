using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Reports;

public record GetProductReportQuery(
    int ProductId,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<ProductDetailReportDto?>;

public class GetProductReportHandler : IRequestHandler<GetProductReportQuery, ProductDetailReportDto?>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetProductReportHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ProductDetailReportDto?> Handle(
        GetProductReportQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string prodSql = """
            SELECT
                p.Id AS ProductId,
                p.Name AS ProductName,
                p.CategoryId,
                COALESCE(c.Name, '') AS CategoryName,
                p.SellingPrice,
                p.Quantity AS CurrentStock,
                p.Unit,
                p.IsActive
            FROM Product p
            LEFT JOIN Category c ON c.Id = p.CategoryId
            WHERE p.Id = @ProductId;
            """;

        var prod = await connection.QuerySingleOrDefaultAsync<ProductDetailReportDto>(
            new CommandDefinition(prodSql, new { request.ProductId }, cancellationToken: cancellationToken));

        if (prod is null)
            return null;

        var fromDate = request.FromDate?.Date;
        var toDateExclusive = request.ToDate?.Date.AddDays(1);

        const string salesSql = """
            SELECT
                COALESCE(SUM(ii.Quantity), 0) AS GrossQuantitySold,
                COALESCE(SUM(ii.LineTotal * (1 - i.DiscountPercentage / 100.0)), 0) AS GrossRevenue,
                COALESCE((
                    SELECT SUM(r.QuantityReturned)
                    FROM Returns r
                    INNER JOIN Invoices orig_i ON orig_i.Id = r.OriginalInvoiceId
                    WHERE r.ProductId = @ProductId
                      AND (@FromDate IS NULL OR orig_i.Date >= @FromDate)
                      AND (@ToDateExclusive IS NULL OR orig_i.Date < @ToDateExclusive)
                ), 0) AS QuantityReturned,
                COALESCE((
                    SELECT SUM(r.QuantityReturned * ii2.UnitPriceSnapshot * (1 - orig_i.DiscountPercentage / 100.0))
                    FROM Returns r
                    INNER JOIN Invoices orig_i ON orig_i.Id = r.OriginalInvoiceId
                    INNER JOIN InvoiceItems ii2 ON ii2.InvoiceId = orig_i.Id AND ii2.ProductId = r.ProductId
                    WHERE r.ProductId = @ProductId
                      AND (@FromDate IS NULL OR orig_i.Date >= @FromDate)
                      AND (@ToDateExclusive IS NULL OR orig_i.Date < @ToDateExclusive)
                ), 0) AS ReturnedRevenue
            FROM InvoiceItems ii
            INNER JOIN Invoices i ON i.Id = ii.InvoiceId
            WHERE ii.ProductId = @ProductId
              AND (@FromDate IS NULL OR i.Date >= @FromDate)
              AND (@ToDateExclusive IS NULL OR i.Date < @ToDateExclusive);
            """;

        var salesStats = await connection.QuerySingleAsync(
            new CommandDefinition(
                salesSql,
                new
                {
                    request.ProductId,
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive
                },
                cancellationToken: cancellationToken));

        const string rankSql = """
            SELECT
                p.Id AS ProductId,
                (COALESCE(SUM(ii.Quantity), 0) - COALESCE(ret.TotalReturned, 0)) AS NetQuantitySold
            FROM Product p
            LEFT JOIN InvoiceItems ii ON ii.ProductId = p.Id
            LEFT JOIN Invoices i ON i.Id = ii.InvoiceId AND (@FromDate IS NULL OR i.Date >= @FromDate) AND (@ToDateExclusive IS NULL OR i.Date < @ToDateExclusive)
            LEFT JOIN (
                SELECT r.ProductId, SUM(r.QuantityReturned) AS TotalReturned
                FROM Returns r
                INNER JOIN Invoices orig_i ON orig_i.Id = r.OriginalInvoiceId
                WHERE (@FromDate IS NULL OR orig_i.Date >= @FromDate) AND (@ToDateExclusive IS NULL OR orig_i.Date < @ToDateExclusive)
                GROUP BY r.ProductId
            ) ret ON ret.ProductId = p.Id
            GROUP BY p.Id
            ORDER BY NetQuantitySold DESC, p.Id ASC;
            """;

        var rankRows = (await connection.QueryAsync<(int ProductId, int NetQty)>(
            new CommandDefinition(
                rankSql,
                new
                {
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive
                },
                cancellationToken: cancellationToken))).ToList();

        var rank = rankRows.FindIndex(x => x.ProductId == request.ProductId) + 1;
        if (rank <= 0) rank = rankRows.Count;

        prod.FromDate = request.FromDate?.Date;
        prod.ToDate = request.ToDate?.Date;
        prod.GrossQuantitySold = (int)(salesStats.GrossQuantitySold ?? 0);
        prod.QuantityReturned = (int)(salesStats.QuantityReturned ?? 0);
        prod.NetQuantitySold = prod.GrossQuantitySold - prod.QuantityReturned;
        prod.GrossRevenue = (decimal)(salesStats.GrossRevenue ?? 0m);
        prod.ReturnedRevenue = (decimal)(salesStats.ReturnedRevenue ?? 0m);
        prod.NetRevenue = prod.GrossRevenue - prod.ReturnedRevenue;
        prod.SalesRank = rank;
        prod.TotalProductsCount = rankRows.Count;

        return prod;
    }
}
