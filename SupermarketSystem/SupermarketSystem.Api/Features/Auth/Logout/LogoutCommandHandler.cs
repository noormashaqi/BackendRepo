using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public LogoutCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // بيسكر آخر سطر Attendance مفتوح (اللي لسا ماله LogoutTime) لنفس الموظف
        const string sql = @"
            UPDATE AttendanceLog
            SET LogoutTime = @LogoutTime
            WHERE EmployeeId = @EmployeeId
              AND LogoutTime IS NULL
            ORDER BY LoginTime DESC
            LIMIT 1;";

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            EmployeeId = request.EmployeeId,
            LogoutTime = DateTime.UtcNow
        });

        return affectedRows > 0;
    }
}
