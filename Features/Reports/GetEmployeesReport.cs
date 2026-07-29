using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Reports;

public record GetEmployeesReportQuery(
    bool? ActiveOnly,
    string? Role
) : IRequest<EmployeesReportDto>;

public class GetEmployeesReportHandler : IRequestHandler<GetEmployeesReportQuery, EmployeesReportDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEmployeesReportHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EmployeesReportDto> Handle(
        GetEmployeesReportQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                e.Id AS EmployeeId,
                e.FullName,
                e.Username,
                e.Role,
                e.IsActive,
                e.CreatedAt,
                COUNT(ep.Id) AS PermissionCount
            FROM Employees e
            LEFT JOIN EmployeePermissions ep
                ON ep.EmployeeId = e.Id
            WHERE (@ActiveOnly IS NULL OR e.IsActive = @ActiveOnly)
              AND (@Role IS NULL OR e.Role = @Role)
            GROUP BY e.Id, e.FullName, e.Username, e.Role, e.IsActive, e.CreatedAt
            ORDER BY e.FullName, e.Id;
            """;

        var rows = (await connection.QueryAsync<EmployeeReportItemDto>(
            new CommandDefinition(
                sql,
                new
                {
                    request.ActiveOnly,
                    Role = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role
                },
                cancellationToken: cancellationToken))).ToList();

        return new EmployeesReportDto
        {
            ActiveOnly = request.ActiveOnly,
            Role = request.Role,
            EmployeeCount = rows.Count,
            Employees = rows
        };
    }
}
