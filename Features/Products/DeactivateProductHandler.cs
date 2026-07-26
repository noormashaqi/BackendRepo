using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Unit>
{
    private readonly IDbConnectionFactory _dbFactory;

    public DeactivateProductHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Unit> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(
            "UPDATE Product SET IsActive = 0 WHERE Id = @Id",
            new { request.Id });

        return Unit.Value;
    }
}