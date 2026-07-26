using MediatR;

namespace SupermarketSystem.Api.Services.Products;

public class DeactivateProductCommand : IRequest<Unit>
{
    public int Id { get; set; }
}