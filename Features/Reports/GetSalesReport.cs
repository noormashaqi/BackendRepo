using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Reports;

public record GetSalesReportQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    int? EmployeeId
) : IRequest<SalesReportDto>;

public class GetSalesReportHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetSalesReportHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SalesReportDto> Handle(
        GetSalesReportQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                i.Id AS InvoiceId,
                i.InvoiceNumber,
                i.EmployeeId,
                e.FullName AS EmployeeName,
                i.Date,
                i.TotalBeforeDiscount,
                i.DiscountPercentage,
                i.TotalAfterDiscount,
                i.HasReturn
            FROM Invoices i
            INNER JOIN Employees e
                ON e.Id = i.EmployeeId
            WHERE (@FromDate IS NULL OR i.Date >= @FromDate)
              AND (@ToDateExclusive IS NULL OR i.Date < @ToDateExclusive)
              AND (@EmployeeId IS NULL OR i.EmployeeId = @EmployeeId)
            ORDER BY i.Date DESC, i.Id DESC;
            """;

        var fromDate = request.FromDate?.Date;
        var toDateExclusive = request.ToDate?.Date.AddDays(1);

        var rows = (await connection.QueryAsync<SalesReportInvoiceDto>(
            new CommandDefinition(
                sql,
                new
                {
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive,
                    request.EmployeeId
                },
                cancellationToken: cancellationToken))).ToList();

        return new SalesReportDto
        {
            FromDate = request.FromDate?.Date,
            ToDate = request.ToDate?.Date,
            EmployeeId = request.EmployeeId,
            InvoiceCount = rows.Count,
            TotalSalesBeforeDiscount = rows.Sum(x => x.TotalBeforeDiscount),
            TotalDiscountAmount = rows.Sum(x => x.TotalBeforeDiscount - x.TotalAfterDiscount),
            TotalSalesAfterDiscount = rows.Sum(x => x.TotalAfterDiscount),
            Invoices = rows
        };
    }
}
