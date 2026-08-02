using Dapper;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Permissions;

public class PermissionService : IPermissionService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PermissionService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> HasPermissionAsync(
        long employeeId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string employeeSql = """
            SELECT Role, IsActive
            FROM Employees
            WHERE Id = @EmployeeId
            LIMIT 1;
            """;

        var employee =
            await connection.QuerySingleOrDefaultAsync<EmployeeForPermission>(
                new CommandDefinition(
                    employeeSql,
                    new { EmployeeId = employeeId },
                    cancellationToken: cancellationToken));


        // الموظف غير موجود أو موقف
        if (employee is null || !employee.IsActive)
        {
            return false;
        }


        // Admin عنده كل الصلاحيات
        if (employee.Role == "Admin")
        {
            return true;
        }


        const string permissionSql = """
            SELECT COUNT(*)
            FROM EmployeePermissions
            WHERE EmployeeId = @EmployeeId
            AND PermissionKey = @PermissionKey;
            """;


        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                permissionSql,
                new
                {
                    EmployeeId = employeeId,
                    PermissionKey = permissionKey
                },
                cancellationToken: cancellationToken));


        return count > 0;
    }


    private class EmployeeForPermission
    {
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}