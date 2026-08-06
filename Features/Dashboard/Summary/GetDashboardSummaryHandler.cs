using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Dashboard.Summary;

public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, GetDashboardSummaryResult>
{
    // مؤقتًا رقم ثابت، لأنه ما في عمود LowStockThreshold بجدول Product حاليًا
    private const int LowStockThreshold = 5;

    private readonly IDbConnectionFactory _connectionFactory;

    public GetDashboardSummaryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetDashboardSummaryResult> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // 1) مبيعات اليوم + عدد الفواتير (بنفس الاستعلام لتقليل الرحلات للداتابيز)
        var salesRow = await connection.QuerySingleAsync<(decimal TodayTotalSales, int TodayInvoiceCount)>(
            new CommandDefinition(
                @"SELECT
                    COALESCE(SUM(TotalAfterDiscount), 0) AS TodayTotalSales,
                    COUNT(*) AS TodayInvoiceCount
                  FROM invoices
                  WHERE DATE(Date) = CURDATE()",
                cancellationToken: cancellationToken));

        // 2) عدد المنتجات منتهية المخزون / قليلة المخزون
        var stockRow = await connection.QuerySingleAsync<(int OutOfStockCount, int LowStockCount)>(
            new CommandDefinition(
                @"SELECT
                    SUM(CASE WHEN Quantity = 0 THEN 1 ELSE 0 END) AS OutOfStockCount,
                    SUM(CASE WHEN Quantity > 0 AND Quantity <= @Threshold THEN 1 ELSE 0 END) AS LowStockCount
                  FROM product
                  WHERE IsActive = 1",
                new { Threshold = LowStockThreshold },
                cancellationToken: cancellationToken));

        return new GetDashboardSummaryResult(
            salesRow.TodayTotalSales,
            salesRow.TodayInvoiceCount,
            stockRow.LowStockCount,
            stockRow.OutOfStockCount);
    }
}