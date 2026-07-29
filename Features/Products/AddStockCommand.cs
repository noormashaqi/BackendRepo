using MediatR;

namespace SupermarketSystem.Api.Services.Products;

public class AddStockCommand : IRequest<AddStockResult>
{
    public int ProductId { get; set; }
    public int QuantityAdded { get; set; }
    public long EmployeeId { get; set; }
}

public class AddStockResult
{
    public int ProductId { get; set; }
    public int NewQuantity { get; set; }
}