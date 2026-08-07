using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

namespace SupermarketSystem.Api.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        IJwtService jwtService,
        ILogger<LogoutCommandHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _jwtService = jwtService;
        _logger = logger;
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

        // 1️⃣ إغلاق آخر سطر حضور مفتوح للموظف (إن وجد)
        const string closeAttendanceSql = @"
            UPDATE AttendanceLogs
            SET LogoutTime = @LogoutTime
            WHERE EmployeeId = @EmployeeId
              AND LogoutTime IS NULL
            ORDER BY LoginTime DESC
            LIMIT 1;";

        var attendanceRowsAffected = await connection.ExecuteAsync(
            closeAttendanceSql,
            new
            {
                request.EmployeeId,
                LogoutTime = DateTime.UtcNow
            },
            transaction);

        if (attendanceRowsAffected == 0)
        {
            _logger.LogInformation(
                "No active attendance record found for EmployeeId {EmployeeId} during logout. Proceeding with token revocation.",
                request.EmployeeId);
        }

        // 2️⃣ إلغاء صلاحية الـ Refresh Token (Revoke)
        const string revokeTokenSql = @"
            UPDATE RefreshTokens
            SET RevokedAt = @RevokedAt
            WHERE EmployeeId = @EmployeeId
              AND TokenHash = @TokenHash
              AND RevokedAt IS NULL;";

        await connection.ExecuteAsync(
            revokeTokenSql,
            new
            {
                request.EmployeeId,
                TokenHash = refreshTokenHash,
                RevokedAt = DateTime.UtcNow
            },
            transaction);

        transaction.Commit();

        return LogoutResult.Ok();
    }
}