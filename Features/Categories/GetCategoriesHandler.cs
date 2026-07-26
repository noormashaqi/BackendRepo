using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Categories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IDbConnectionFactory _dbFactory;

    public GetCategoriesHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var categories = await connection.QueryAsync<CategoryDto>(
            "SELECT Id, Name FROM Category ORDER BY Name");

        return categories.ToList();
    }
}