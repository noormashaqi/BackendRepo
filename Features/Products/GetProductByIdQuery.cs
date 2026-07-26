using MediatR;
using SupermarketSystem.Api.DTOs;

namespace SupermarketSystem.Api.Services.Products;

public class GetProductByIdQuery : IRequest<ProductDto?>
{
    public int Id { get; set; }
}