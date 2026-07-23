using MediatR;
using SupermarketSystem.Api.DTOs;

namespace SupermarketSystem.Api.Services.Categories;

public class GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}