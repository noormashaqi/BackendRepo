using Dapper;
using MediatR;
using SupermarketSystem.Api.Features.Auth;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ResetPasswordCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ResetPasswordResult> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0)
            return ResetPasswordResult.Fail("Employee id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return ResetPasswordResult.Fail("Current password is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return ResetPasswordResult.Fail("New password is required.");

        if (request.NewPassword.Length < 8)
            return ResetPasswordResult.Fail("New password must contain at least 8 characters.");

        var employee = await AuthDataAccess.GetEmployeeByIdAsync(
            _connectionFactory,
            request.EmployeeId,
            cancellationToken);

        if (employee is null || !employee.IsActive)
            return ResetPasswordResult.Fail("Employee not found or inactive.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, employee.PasswordHash))
            return ResetPasswordResult.Fail("Current password is incorrect.");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        using var connection = _connectionFactory.CreateConnection();

        const string updatePasswordSql = """
            UPDATE Employees
            SET PasswordHash = @PasswordHash
            WHERE Id = @EmployeeId;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                updatePasswordSql,
                new
                {
                    EmployeeId = request.EmployeeId,
                    PasswordHash = newPasswordHash
                },
                cancellationToken: cancellationToken));

        const string revokeRefreshTokensSql = """
            UPDATE RefreshTokens
            SET RevokedAt = UTC_TIMESTAMP()
            WHERE EmployeeId = @EmployeeId
              AND RevokedAt IS NULL;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                revokeRefreshTokensSql,
                new { EmployeeId = request.EmployeeId },
                cancellationToken: cancellationToken));

        return ResetPasswordResult.Ok();
    }
}
