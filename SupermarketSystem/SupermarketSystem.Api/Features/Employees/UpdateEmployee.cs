using Dapper;
using MediatR;
using MySqlConnector;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Employees;

public class UpdateEmployeeCommand : IRequest<UpdateEmployeeResult>
{
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}

public class UpdateEmployeeResult
{
    public bool Success { get; set; }

    public string? ErrorCode { get; set; }

    public string? Message { get; set; }

    public EmployeeResponse? Employee { get; set; }

    public static UpdateEmployeeResult Succeeded(
        EmployeeResponse employee)
    {
        return new UpdateEmployeeResult
        {
            Success = true,
            Employee = employee
        };
    }

    public static UpdateEmployeeResult Failed(
        string errorCode,
        string message)
    {
        return new UpdateEmployeeResult
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}

public class UpdateEmployeeHandler
    : IRequestHandler<UpdateEmployeeCommand, UpdateEmployeeResult>
{
    private static readonly string[] AllowedRoles =
    [
        "Admin",
        "Cashier",
        "InventoryEmployee"
    ];

    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateEmployeeHandler(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UpdateEmployeeResult> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var username = request.Username?.Trim() ?? string.Empty;
        var role = request.Role?.Trim() ?? string.Empty;

        var validationMessage = Validate(
            request.Id,
            fullName,
            username,
            role);

        if (validationMessage is not null)
        {
            return UpdateEmployeeResult.Failed(
                "ValidationError",
                validationMessage);
        }

        using var connection = _connectionFactory.CreateConnection();

        const string employeeExistsSql = """
            SELECT COUNT(1)
            FROM Employees
            WHERE Id = @Id;
            """;

        var employeeExistsCommand = new CommandDefinition(
            employeeExistsSql,
            new { request.Id },
            cancellationToken: cancellationToken);

        var employeeExists =
            await connection.ExecuteScalarAsync<int>(
                employeeExistsCommand) > 0;

        if (!employeeExists)
        {
            return UpdateEmployeeResult.Failed(
                "NotFound",
                "Employee not found.");
        }

        const string usernameExistsSql = """
            SELECT COUNT(1)
            FROM Employees
            WHERE LOWER(Username) = LOWER(@Username)
              AND Id <> @Id;
            """;

        var usernameExistsCommand = new CommandDefinition(
            usernameExistsSql,
            new
            {
                Username = username,
                request.Id
            },
            cancellationToken: cancellationToken);

        var usernameExists =
            await connection.ExecuteScalarAsync<int>(
                usernameExistsCommand) > 0;

        if (usernameExists)
        {
            return UpdateEmployeeResult.Failed(
                "UsernameAlreadyExists",
                "Username already exists.");
        }

        const string updateSql = """
            UPDATE Employees
            SET
                FullName = @FullName,
                Username = @Username,
                Role = @Role
            WHERE Id = @Id;
            """;

        try
        {
            var updateCommand = new CommandDefinition(
                updateSql,
                new
                {
                    request.Id,
                    FullName = fullName,
                    Username = username,
                    Role = role
                },
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(updateCommand);
        }
        catch (MySqlException exception)
            when (exception.Number == 1062)
        {
            return UpdateEmployeeResult.Failed(
                "UsernameAlreadyExists",
                "Username already exists.");
        }

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
            new { request.Id },
            cancellationToken: cancellationToken);

        var employee =
            await connection.QuerySingleAsync<EmployeeResponse>(
                selectCommand);

        return UpdateEmployeeResult.Succeeded(employee);
    }

    private static string? Validate(
        long id,
        string fullName,
        string username,
        string role)
    {
        if (id <= 0)
        {
            return "Employee id must be greater than zero.";
        }

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

        if (!AllowedRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
        {
            return "Role must be Admin, Cashier, or InventoryEmployee.";
        }

        return null;
    }
}