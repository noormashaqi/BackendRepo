using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Products;

public class AddStockHandler : IRequestHandler<AddStockCommand, AddStockResult>
{
    private readonly IDbConnectionFactory _dbFactory;

    public AddStockHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<AddStockResult> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                "UPDATE Product SET Quantity = Quantity + @QuantityAdded WHERE Id = @ProductId",
                new { request.QuantityAdded, request.ProductId },
                transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO StockHistory (ProductId, QuantityAdded, EmployeeId, Date)
                VALUES (@ProductId, @QuantityAdded, @EmployeeId, UTC_TIMESTAMP())",
                new { request.ProductId, request.QuantityAdded, request.EmployeeId },
                transaction);

            var newQuantity = await connection.QuerySingleAsync<int>(
                "SELECT Quantity FROM Product WHERE Id = @ProductId",
                new { request.ProductId },
                transaction);

            transaction.Commit();

            return new AddStockResult
            {
                ProductId = request.ProductId,
                NewQuantity = newQuantity
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}