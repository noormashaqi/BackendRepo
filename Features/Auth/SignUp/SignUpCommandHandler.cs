using Dapper;
using MediatR;
using MySqlConnector;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Features.Auth;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

namespace SupermarketSystem.Api.Features.Auth.SignUp;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, SignUpResult>
{
    private static readonly string[] AllowedRoles =
    [
        "Cashier",
        "InventoryEmployee"
    ];

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IJwtService _jwtService;

    public SignUpCommandHandler(
        IDbConnectionFactory connectionFactory,
        IJwtService jwtService)
    {
        _connectionFactory = connectionFactory;
        _jwtService = jwtService;
    }

    public async Task<SignUpResult> Handle(
        SignUpCommand request,
        CancellationToken cancellationToken)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var username = request.Username?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var role = string.IsNullOrWhiteSpace(request.Role) ? "Cashier" : request.Role.Trim();

        var validationMessage = Validate(fullName, username, password, role);
        if (validationMessage is not null)
        {
            return SignUpResult.Fail("ValidationError", validationMessage);
        }

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string usernameExistsSql = """
            SELECT COUNT(1)
            FROM Employees
            WHERE LOWER(Username) = LOWER(@Username);
            """;

        var usernameExists = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                usernameExistsSql,
                new { Username = username },
                transaction: transaction,
                cancellationToken: cancellationToken)) > 0;

        if (usernameExists)
        {
            transaction.Rollback();
            return SignUpResult.Fail("UsernameAlreadyExists", "Username already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        const string insertEmployeeSql = """
            INSERT INTO Employees
            (
                FullName,
                Username,
                PasswordHash,
                Role,
                IsActive,
                CreatedAt
            )
            VALUES
            (
                @FullName,
                @Username,
                @PasswordHash,
                @Role,
                TRUE,
                UTC_TIMESTAMP()
            );

            SELECT LAST_INSERT_ID();
            """;

        try
        {
            var employeeId = await connection.QuerySingleAsync<long>(
                new CommandDefinition(
                    insertEmployeeSql,
                    new
                    {
                        FullName = fullName,
                        Username = username,
                        PasswordHash = passwordHash,
                        Role = role
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var employee = new Employee
            {
                Id = employeeId,
                FullName = fullName,
                Username = username,
                PasswordHash = passwordHash,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var permissions = await AuthDataAccess.GetPermissionsAsync(
                _connectionFactory,
                employee,
                cancellationToken);

            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenHash = _jwtService.ComputeRefreshTokenHash(refreshToken);
            var refreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiry();

            const string insertRefreshTokenSql = """
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
                    insertRefreshTokenSql,
                    new
                    {
                        EmployeeId = employeeId,
                        TokenHash = refreshTokenHash,
                        ExpiresAt = refreshTokenExpiresAt
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            transaction.Commit();

            var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(employee, permissions);

            return SignUpResult.Ok(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                EmployeeId = employee.Id,
                FullName = employee.FullName,
                Username = employee.Username,
                Role = employee.Role,
                Permissions = permissions
            });
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            transaction.Rollback();
            return SignUpResult.Fail("UsernameAlreadyExists", "Username already exists.");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static string? Validate(
        string fullName,
        string username,
        string password,
        string role)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Full name is required.";

        if (fullName.Length > 150)
            return "Full name must not exceed 150 characters.";

        if (string.IsNullOrWhiteSpace(username))
            return "Username is required.";

        if (username.Length > 100)
            return "Username must not exceed 100 characters.";

        if (string.IsNullOrWhiteSpace(password))
            return "Password is required.";

        if (password.Length < 8)
            return "Password must contain at least 8 characters.";

        if (password.Length > 100)
            return "Password must not exceed 100 characters.";

        if (!AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            return "Role must be Cashier or InventoryEmployee.";

        return null;
    }
}
