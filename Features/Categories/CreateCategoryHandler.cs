using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Services.Categories;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IDbConnectionFactory _dbFactory;

    public CreateCategoryHandler(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        using var connection = _dbFactory.CreateConnection();

        var newId = await connection.QuerySingleAsync<int>(
            "INSERT INTO Category (Name) VALUES (@Name); SELECT LAST_INSERT_ID();",
            new { request.Name });

        return new CategoryDto { Id = newId, Name = request.Name };
    }
}