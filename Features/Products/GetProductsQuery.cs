using MediatR;
using SupermarketSystem.Api.DTOs;

namespace SupermarketSystem.Api.Services.Products;

public class GetProductsQuery : IRequest<List<ProductDto>>
{
    public int? CategoryId { get; set; }
    public bool ActiveOnly { get; set; } = true;
}