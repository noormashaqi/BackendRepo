using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Features.Auth;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

namespace SupermarketSystem.Api.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IDbConnectionFactory connectionFactory,
        IJwtService jwtService)
    {
        _connectionFactory = connectionFactory;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return RefreshTokenResult.Fail("Refresh token is required.");
        }

        var refreshTokenHash = _jwtService.ComputeRefreshTokenHash(request.RefreshToken);

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string tokenSql = """
            SELECT Id, EmployeeId, TokenHash, ExpiresAt, CreatedAt, RevokedAt, ReplacedByTokenHash
            FROM RefreshTokens
            WHERE TokenHash = @TokenHash
            LIMIT 1
            FOR UPDATE;
            """;

        var storedToken = await connection.QuerySingleOrDefaultAsync<global::RefreshToken>(
            new CommandDefinition(
                tokenSql,
                new { TokenHash = refreshTokenHash },
                transaction: transaction,
                cancellationToken: cancellationToken));

        if (storedToken is null || storedToken.RevokedAt.HasValue || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            transaction.Rollback();
            return RefreshTokenResult.Fail("Invalid or expired refresh token.");
        }

        var employee = await AuthDataAccess.GetEmployeeByIdAsync(
            _connectionFactory,
            storedToken.EmployeeId,
            cancellationToken);

        if (employee is null || !employee.IsActive)
        {
            transaction.Rollback();
            return RefreshTokenResult.Fail("Employee is inactive or not found.");
        }

        var permissions = await AuthDataAccess.GetPermissionsAsync(
            _connectionFactory,
            employee,
            cancellationToken);

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.ComputeRefreshTokenHash(newRefreshToken);
        var newRefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiry();

        const string revokeSql = """
            UPDATE RefreshTokens
            SET RevokedAt = UTC_TIMESTAMP(),
                ReplacedByTokenHash = @ReplacedByTokenHash
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                revokeSql,
                new
                {
                    storedToken.Id,
                    ReplacedByTokenHash = newRefreshTokenHash
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

        const string insertSql = """
            INSERT INTO RefreshTokens
            (
                EmployeeId,
                TokenHash,
                ExpiresAt,
                CreatedAt,
                RevokedAt,
                ReplacedByTokenHash
            )
            VALUES
            (
                @EmployeeId,
                @TokenHash,
                @ExpiresAt,
                UTC_TIMESTAMP(),
                NULL,
                NULL
            );
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                insertSql,
                new
                {
                    EmployeeId = employee.Id,
                    TokenHash = newRefreshTokenHash,
                    ExpiresAt = newRefreshTokenExpiresAt
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

        transaction.Commit();

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(employee, permissions);

        return RefreshTokenResult.Ok(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            Username = employee.Username,
            Role = employee.Role,
            Permissions = permissions
        });
    }
}
