using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Reports;

public record GetAttendanceReportQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    long? EmployeeId
) : IRequest<AttendanceReportDto>;

public class GetAttendanceReportHandler : IRequestHandler<GetAttendanceReportQuery, AttendanceReportDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAttendanceReportHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AttendanceReportDto> Handle(
        GetAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                a.Id AS AttendanceLogId,
                a.EmployeeId,
                e.FullName AS EmployeeName,
                a.LoginTime,
                a.LogoutTime,
                CASE
                    WHEN a.LogoutTime IS NULL THEN NULL
                    ELSE TIMESTAMPDIFF(SECOND, a.LoginTime, a.LogoutTime) / 3600.0
                END AS ShiftDurationHours
            FROM AttendanceLogs a
            INNER JOIN Employees e
                ON e.Id = a.EmployeeId
            WHERE (@FromDate IS NULL OR a.LoginTime >= @FromDate)
              AND (@ToDateExclusive IS NULL OR a.LoginTime < @ToDateExclusive)
              AND (@EmployeeId IS NULL OR a.EmployeeId = @EmployeeId)
            ORDER BY a.LoginTime DESC, a.Id DESC;
            """;

        var fromDate = request.FromDate?.Date;
        var toDateExclusive = request.ToDate?.Date.AddDays(1);

        var rows = (await connection.QueryAsync<AttendanceReportEntryDto>(
            new CommandDefinition(
                sql,
                new
                {
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive,
                    request.EmployeeId
                },
                cancellationToken: cancellationToken))).ToList();

        return new AttendanceReportDto
        {
            FromDate = request.FromDate?.Date,
            ToDate = request.ToDate?.Date,
            EmployeeId = request.EmployeeId,
            EntryCount = rows.Count,
            Entries = rows
        };
    }
}
