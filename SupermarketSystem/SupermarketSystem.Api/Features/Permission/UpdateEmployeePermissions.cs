using Dapper;
using MediatR;
using SupermarketSystem.Api.Constants;
using SupermarketSystem.Api.Dtos.Permissions;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Permission;
public record UpdateEmployeePermissionsCommand(
    long EmployeeId,
    UpdateEmployeePermissionsRequest Request
) : IRequest<UpdateEmployeePermissionsResult>;

public class UpdateEmployeePermissionsHandler
    : IRequestHandler<
        UpdateEmployeePermissionsCommand,
        UpdateEmployeePermissionsResult>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateEmployeePermissionsHandler(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UpdateEmployeePermissionsResult> Handle(
        UpdateEmployeePermissionsCommand command,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string employeeSql = """
                SELECT Id, Role
                FROM Employees
                WHERE Id = @EmployeeId;
                """;

            var employee = await connection
                .QuerySingleOrDefaultAsync<EmployeeInfo>(
                    employeeSql,
                    new { command.EmployeeId },
                    transaction);

            if (employee is null)
            {
                return UpdateEmployeePermissionsResult.NotFound();
            }

            if (string.Equals(
                employee.Role,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return UpdateEmployeePermissionsResult.Failure(
                    "Admin permissions cannot be modified.");
            }

            var permissionsToAdd = command.Request.PermissionsToAdd
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var permissionsToRemove = command.Request.PermissionsToRemove
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allRequestedPermissions = permissionsToAdd
                .Concat(permissionsToRemove)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var invalidPermissions = allRequestedPermissions
                .Where(permission => !PermissionKeys.IsValid(permission))
                .ToList();

            if (invalidPermissions.Count > 0)
            {
                return UpdateEmployeePermissionsResult.Failure(
                    $"Invalid permissions: {string.Join(", ", invalidPermissions)}");
            }

            const string insertSql = """
                INSERT IGNORE INTO EmployeePermissions
                    (EmployeeId, PermissionKey)
                VALUES
                    (@EmployeeId, @PermissionKey);
                """;

            foreach (var permission in permissionsToAdd)
            {
                await connection.ExecuteAsync(
                    insertSql,
                    new
                    {
                        command.EmployeeId,
                        PermissionKey = permission
                    },
                    transaction);
            }

            const string deleteSql = """
                DELETE FROM EmployeePermissions
                WHERE EmployeeId = @EmployeeId
                  AND PermissionKey = @PermissionKey;
                """;

            foreach (var permission in permissionsToRemove)
            {
                await connection.ExecuteAsync(
                    deleteSql,
                    new
                    {
                        command.EmployeeId,
                        PermissionKey = permission
                    },
                    transaction);
            }

            transaction.Commit();

            return UpdateEmployeePermissionsResult.Success();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private sealed class EmployeeInfo
    {
        public long Id { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}

public class UpdateEmployeePermissionsResult
{
    public bool IsSuccess { get; private init; }

    public bool IsNotFound { get; private init; }

    public string? ErrorMessage { get; private init; }

    public static UpdateEmployeePermissionsResult Success()
    {
        return new UpdateEmployeePermissionsResult
        {
            IsSuccess = true
        };
    }

    public static UpdateEmployeePermissionsResult NotFound()
    {
        return new UpdateEmployeePermissionsResult
        {
            IsNotFound = true
        };
    }

    public static UpdateEmployeePermissionsResult Failure(string message)
    {
        return new UpdateEmployeePermissionsResult
        {
            ErrorMessage = message
        };
    }
}