using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Employees;

public record GetEmployeesQuery : IRequest<IEnumerable<EmployeeResponse>>;

public class GetEmployeesHandler
    : IRequestHandler<GetEmployeesQuery, IEnumerable<EmployeeResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEmployeesHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<EmployeeResponse>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                Id,
                FullName,
                Username,
                Role,
                IsActive,
                CreatedAt
            FROM Employees
            ORDER BY CreatedAt DESC, Id DESC;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<EmployeeResponse>(command);
    }
}