using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

namespace SupermarketSystem.Api.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IJwtService _jwtService;

    public LogoutCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IJwtService jwtService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _jwtService = jwtService;
    }

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0)
            return LogoutResult.Fail("Employee id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return LogoutResult.Fail("Refresh token is required.");

        var refreshTokenHash = _jwtService.ComputeRefreshTokenHash(request.RefreshToken);

        using var connection = _dbConnectionFactory.CreateConnection();

        // بيسكر آخر سطر Attendance مفتوح (اللي لسا ماله LogoutTime) لنفس الموظف
        const string sql = @"
            UPDATE AttendanceLog
            SET LogoutTime = @LogoutTime
            WHERE EmployeeId = @EmployeeId
              AND LogoutTime IS NULL
            ORDER BY LoginTime DESC
            LIMIT 1;
            """;

        await connection.ExecuteAsync(
            closeAttendanceSql,
            new
            {
                request.EmployeeId,
                LogoutTime = DateTime.UtcNow
            },
            transaction);

        transaction.Commit();

        return LogoutResult.Ok();
    }
}
