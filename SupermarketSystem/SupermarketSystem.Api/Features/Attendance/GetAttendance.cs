using Dapper;
using MediatR;
using SupermarketSystem.Api.Dtos.Attendance;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Attendance;

public record GetAttendanceQuery(DateTime? Date)
    : IRequest<IReadOnlyCollection<AttendanceResponse>>;

public class GetAttendanceHandler
    : IRequestHandler<GetAttendanceQuery, IReadOnlyCollection<AttendanceResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAttendanceHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<AttendanceResponse>> Handle(
        GetAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                a.Id,
                a.EmployeeId,
                e.FullName AS EmployeeName,
                a.LoginTime,
                a.LogoutTime
            FROM AttendanceLogs a
            INNER JOIN Employees e
                ON e.Id = a.EmployeeId
            WHERE (
                @Date IS NULL
                OR (
                    a.LoginTime >= @StartDate
                    AND a.LoginTime < @EndDate
                )
            )
            ORDER BY a.LoginTime DESC;
            """;

        var startDate = request.Date?.Date;
        var endDate = startDate?.AddDays(1);

        var result = await connection.QueryAsync<AttendanceResponse>(
            sql,
            new
            {
                Date = request.Date,
                StartDate = startDate,
                EndDate = endDate
            });

        return result.ToList();
    }
}