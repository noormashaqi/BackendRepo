using MediatR;
using SupermarketSystem.Api.DTOs;

namespace SupermarketSystem.Api.Services.Categories;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; set; } = string.Empty;
}