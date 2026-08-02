using MediatR;
using SupermarketSystem.Api.DTOs;

namespace SupermarketSystem.Api.Services.Products;

public class GetOutOfStockProductsQuery : IRequest<List<ProductDto>>
{
}