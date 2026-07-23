using Dapper;
using MediatR;
using SupermarketSystem.Api.Constants;
using SupermarketSystem.Api.Dtos.Permissions;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Permission;
public record GetEmployeePermissionsQuery(long EmployeeId)
    : IRequest<EmployeePermissionsResponse?>;

public class GetEmployeePermissionsHandler
    : IRequestHandler<GetEmployeePermissionsQuery, EmployeePermissionsResponse?>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEmployeePermissionsHandler(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EmployeePermissionsResponse?> Handle(
        GetEmployeePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string employeeSql = """
            SELECT
                Id,
                FullName,
                Role
            FROM Employees
            WHERE Id = @EmployeeId;
            """;

        var employee = await connection.QuerySingleOrDefaultAsync<EmployeeInfo>(
            employeeSql,
            new { request.EmployeeId });

        if (employee is null)
        {
            return null;
        }

        IReadOnlyCollection<string> permissions;

        if (string.Equals(
            employee.Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase))
        {
            permissions = PermissionKeys.All.ToArray();
        }
        else
        {
            const string permissionsSql = """
                SELECT PermissionKey
                FROM EmployeePermissions
                WHERE EmployeeId = @EmployeeId
                ORDER BY PermissionKey;
                """;

            var result = await connection.QueryAsync<string>(
                permissionsSql,
                new { request.EmployeeId });

            permissions = result.ToArray();
        }

        return new EmployeePermissionsResponse
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            Role = employee.Role,
            Permissions = permissions
        };
    }

    private sealed class EmployeeInfo
    {
        public long Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}