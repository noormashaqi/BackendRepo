using Dapper;
using MediatR;
using MySqlConnector;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Employees;

public class CreateEmployeeCommand : IRequest<CreateEmployeeResult>
{
    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}

public class CreateEmployeeResult
{
    public bool Success { get; set; }

    public string? ErrorCode { get; set; }

    public string? Message { get; set; }

    public EmployeeResponse? Employee { get; set; }

    public static CreateEmployeeResult Succeeded(EmployeeResponse employee)
    {
        return new CreateEmployeeResult
        {
            Success = true,
            Employee = employee
        };
    }

    public static CreateEmployeeResult Failed(
        string errorCode,
        string message)
    {
        return new CreateEmployeeResult
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}

public class CreateEmployeeHandler
    : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResult>
{
    private static readonly string[] AllowedRoles =
    [
        "Admin",
        "Cashier",
        "InventoryEmployee"
    ];

    private readonly IDbConnectionFactory _connectionFactory;

    public CreateEmployeeHandler(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CreateEmployeeResult> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var username = request.Username?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var role = request.Role?.Trim() ?? string.Empty;

        var validationMessage = Validate(
            fullName,
            username,
            password,
            role);

        if (validationMessage is not null)
        {
            return CreateEmployeeResult.Failed(
                "ValidationError",
                validationMessage);
        }

        using var connection = _connectionFactory.CreateConnection();

        const string usernameExistsSql = """
            SELECT COUNT(1)
            FROM Employees
            WHERE LOWER(Username) = LOWER(@Username);
            """;

        var usernameExistsCommand = new CommandDefinition(
            usernameExistsSql,
            new { Username = username },
            cancellationToken: cancellationToken);

        var usernameExists =
            await connection.ExecuteScalarAsync<int>(
                usernameExistsCommand) > 0;

        if (usernameExists)
        {
            return CreateEmployeeResult.Failed(
                "UsernameAlreadyExists",
                "Username already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        const string insertSql = """
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
            var insertCommand = new CommandDefinition(
                insertSql,
                new
                {
                    FullName = fullName,
                    Username = username,
                    PasswordHash = passwordHash,
                    Role = role
                },
                cancellationToken: cancellationToken);

            var employeeId =
                await connection.QuerySingleAsync<long>(insertCommand);

            const string selectSql = """
                SELECT
                    Id,
                    FullName,
                    Username,
                    Role,
                    IsActive,
                    CreatedAt
                FROM Employees
                WHERE Id = @Id;
                """;

            var selectCommand = new CommandDefinition(
                selectSql,
                new { Id = employeeId },
                cancellationToken: cancellationToken);

            var employee =
                await connection.QuerySingleAsync<EmployeeResponse>(
                    selectCommand);

            return CreateEmployeeResult.Succeeded(employee);
        }
        catch (MySqlException exception)
            when (exception.Number == 1062)
        {
            return CreateEmployeeResult.Failed(
                "UsernameAlreadyExists",
                "Username already exists.");
        }
    }

    private static string? Validate(
        string fullName,
        string username,
        string password,
        string role)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "Full name is required.";
        }

        if (fullName.Length > 150)
        {
            return "Full name must not exceed 150 characters.";
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return "Username is required.";
        }

        if (username.Length > 100)
        {
            return "Username must not exceed 100 characters.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        if (password.Length < 8)
        {
            return "Password must contain at least 8 characters.";
        }

        if (password.Length > 100)
        {
            return "Password must not exceed 100 characters.";
        }

        if (!AllowedRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
        {
            return "Role must be Admin, Cashier, or InventoryEmployee.";
        }

        return null;
    }
}