namespace SupermarketSystem.Api.Features.Dashboard.Summary;

public record GetDashboardSummaryResult(
    decimal TodayTotalSales,
    int TodayInvoiceCount,
    int LowStockCount,
    int OutOfStockCount);