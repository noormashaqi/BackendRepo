using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Reports;

public record GetEmployeeReportQuery(
    long EmployeeId,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<EmployeeDetailReportDto?>;

public class GetEmployeeReportHandler : IRequestHandler<GetEmployeeReportQuery, EmployeeDetailReportDto?>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEmployeeReportHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EmployeeDetailReportDto?> Handle(
        GetEmployeeReportQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string empSql = """
            SELECT Id AS EmployeeId, FullName, Username, Role, IsActive
            FROM Employees
            WHERE Id = @EmployeeId;
            """;

        var emp = await connection.QuerySingleOrDefaultAsync<EmployeeDetailReportDto>(
            new CommandDefinition(empSql, new { request.EmployeeId }, cancellationToken: cancellationToken));

        if (emp is null)
            return null;

        const string invoicesSql = """
            SELECT
                i.Id AS InvoiceId,
                i.InvoiceNumber,
                i.EmployeeId,
                e.FullName AS EmployeeName,
                i.Date,
                i.TotalBeforeDiscount,
                i.DiscountPercentage,
                i.TotalAfterDiscount,
                i.HasReturn,
                COALESCE(SUM(r.QuantityReturned * ii.UnitPriceSnapshot * (1 - i.DiscountPercentage / 100.0)), 0) AS ReturnedAmount,
                i.TotalAfterDiscount - COALESCE(SUM(r.QuantityReturned * ii.UnitPriceSnapshot * (1 - i.DiscountPercentage / 100.0)), 0) AS NetTotal
            FROM Invoices i
            INNER JOIN Employees e
                ON e.Id = i.EmployeeId
            LEFT JOIN Returns r
                ON r.OriginalInvoiceId = i.Id
            LEFT JOIN InvoiceItems ii
                ON ii.InvoiceId = i.Id AND ii.ProductId = r.ProductId
            WHERE i.EmployeeId = @EmployeeId
              AND (@FromDate IS NULL OR i.Date >= @FromDate)
              AND (@ToDateExclusive IS NULL OR i.Date < @ToDateExclusive)
            GROUP BY i.Id, i.InvoiceNumber, i.EmployeeId, e.FullName, i.Date, i.TotalBeforeDiscount, i.DiscountPercentage, i.TotalAfterDiscount, i.HasReturn
            ORDER BY i.Date DESC, i.Id DESC;
            """;

        var fromDate = request.FromDate?.Date;
        var toDateExclusive = request.ToDate?.Date.AddDays(1);

        var invoices = (await connection.QueryAsync<SalesReportInvoiceDto>(
            new CommandDefinition(
                invoicesSql,
                new
                {
                    request.EmployeeId,
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive
                },
                cancellationToken: cancellationToken))).ToList();

        emp.FromDate = request.FromDate?.Date;
        emp.ToDate = request.ToDate?.Date;
        emp.InvoiceCount = invoices.Count;
        emp.GrossSales = invoices.Sum(x => x.TotalAfterDiscount);
        emp.TotalReturnedAmount = invoices.Sum(x => x.ReturnedAmount);
        emp.NetSales = emp.GrossSales - emp.TotalReturnedAmount;
        emp.Invoices = invoices;

        return emp;
    }
}
