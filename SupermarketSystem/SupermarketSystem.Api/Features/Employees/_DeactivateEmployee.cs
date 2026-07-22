using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Employees;

public record DeactivateEmployeeCommand(long Id)
    : IRequest<DeactivateEmployeeResult>;

public class DeactivateEmployeeResult
{
    public bool Success { get; set; }

    public string? ErrorCode { get; set; }

    public string? Message { get; set; }

    public static DeactivateEmployeeResult Succeeded()
    {
        return new DeactivateEmployeeResult
        {
            Success = true,
            Message = "Employee deactivated successfully."
        };
    }

    public static DeactivateEmployeeResult Failed(
        string errorCode,
        string message)
    {
        return new DeactivateEmployeeResult
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}

public class DeactivateEmployeeHandler
    : IRequestHandler<
        DeactivateEmployeeCommand,
        DeactivateEmployeeResult>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeactivateEmployeeHandler(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DeactivateEmployeeResult> Handle(
        DeactivateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return DeactivateEmployeeResult.Failed(
                "ValidationError",
                "Employee id must be greater than zero.");
        }

        using var connection = _connectionFactory.CreateConnection();

        const string selectSql = """
            SELECT IsActive
            FROM Employees
            WHERE Id = @Id;
            """;

        var selectCommand = new CommandDefinition(
            selectSql,
            new { request.Id },
            cancellationToken: cancellationToken);

        var isActive =
            await connection.QuerySingleOrDefaultAsync<bool?>(
                selectCommand);

        if (isActive is null)
        {
            return DeactivateEmployeeResult.Failed(
                "NotFound",
                "Employee not found.");
        }

        if (!isActive.Value)
        {
            return DeactivateEmployeeResult.Failed(
                "AlreadyDeactivated",
                "Employee is already deactivated.");
        }

        const string updateSql = """
            UPDATE Employees
            SET IsActive = FALSE
            WHERE Id = @Id
              AND IsActive = TRUE;
            """;

        var updateCommand = new CommandDefinition(
            updateSql,
            new { request.Id },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(updateCommand);

        return DeactivateEmployeeResult.Succeeded();
    }
}