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
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string revokeRefreshTokenSql = """
            UPDATE RefreshTokens
            SET RevokedAt = UTC_TIMESTAMP()
            WHERE EmployeeId = @EmployeeId
              AND TokenHash = @TokenHash
              AND RevokedAt IS NULL;
            """;

        var revokedRows = await connection.ExecuteAsync(
            revokeRefreshTokenSql,
            new
            {
                request.EmployeeId,
                TokenHash = refreshTokenHash
            },
            transaction);

        if (revokedRows == 0)
        {
            transaction.Rollback();
            return LogoutResult.Fail("Refresh token was not found or already revoked.");
        }

        const string closeAttendanceSql = """
            UPDATE AttendanceLogs
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
