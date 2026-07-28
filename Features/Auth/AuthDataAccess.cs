using Dapper;
using SupermarketSystem.Api.Constants;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Auth;

internal static class AuthDataAccess
{
    public static async Task<Employee?> GetEmployeeByUsernameAsync(
        IDbConnectionFactory connectionFactory,
        string username,
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, FullName, Username, PasswordHash, Role, IsActive, CreatedAt
            FROM Employees
            WHERE Username = @Username
            LIMIT 1;
            """;

        return await connection.QuerySingleOrDefaultAsync<Employee>(
            new CommandDefinition(
                sql,
                new { Username = username },
                cancellationToken: cancellationToken));
    }

    public static async Task<Employee?> GetEmployeeByIdAsync(
        IDbConnectionFactory connectionFactory,
        long employeeId,
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, FullName, Username, PasswordHash, Role, IsActive, CreatedAt
            FROM Employees
            WHERE Id = @EmployeeId
            LIMIT 1;
            """;

        return await connection.QuerySingleOrDefaultAsync<Employee>(
            new CommandDefinition(
                sql,
                new { EmployeeId = employeeId },
                cancellationToken: cancellationToken));
    }

    public static async Task<List<string>> GetPermissionsAsync(
        IDbConnectionFactory connectionFactory,
        Employee employee,
        CancellationToken cancellationToken)
    {
        if (string.Equals(employee.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return PermissionKeys.All.ToList();
        }

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT PermissionKey
            FROM EmployeePermissions
            WHERE EmployeeId = @EmployeeId
            ORDER BY PermissionKey;
            """;

        var permissions = await connection.QueryAsync<string>(
            new CommandDefinition(
                sql,
                new { EmployeeId = employee.Id },
                cancellationToken: cancellationToken));

        return permissions.ToList();
    }
}
