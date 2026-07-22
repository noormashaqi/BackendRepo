using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Employees;

public record GetEmployeeByIdQuery(long Id)
    : IRequest<EmployeeResponse?>;

public class GetEmployeeByIdHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse?>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEmployeeByIdHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EmployeeResponse?> Handle(
        GetEmployeeByIdQuery request,
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
            WHERE Id = @Id;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { request.Id },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<EmployeeResponse>(
            command);
    }
}