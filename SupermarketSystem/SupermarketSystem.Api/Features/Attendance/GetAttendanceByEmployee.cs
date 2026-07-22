using Dapper;
using MediatR;
using SupermarketSystem.Api.Dtos.Attendance;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Attendance;

public record GetAttendanceByEmployeeQuery(long EmployeeId)
    : IRequest<IReadOnlyCollection<AttendanceResponse>>;

public class GetAttendanceByEmployeeHandler
    : IRequestHandler<
        GetAttendanceByEmployeeQuery,
        IReadOnlyCollection<AttendanceResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAttendanceByEmployeeHandler(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<AttendanceResponse>> Handle(
        GetAttendanceByEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string employeeExistsSql = """
            SELECT COUNT(1)
            FROM Employees
            WHERE Id = @EmployeeId;
            """;

        var employeeExists = await connection.ExecuteScalarAsync<bool>(
            employeeExistsSql,
            new { request.EmployeeId });

        if (!employeeExists)
        {
            throw new KeyNotFoundException("Employee was not found.");
        }

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
            WHERE a.EmployeeId = @EmployeeId
            ORDER BY a.LoginTime DESC;
            """;

        var result = await connection.QueryAsync<AttendanceResponse>(
            sql,
            new { request.EmployeeId });

        return result.ToList();
    }
}