using MediatR;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Features.Auth;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Auth.Me;

public class GetMeQuery : IRequest<MeResponseDto?>
{
    public long EmployeeId { get; set; }
}

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, MeResponseDto?>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetMeQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MeResponseDto?> Handle(
        GetMeQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await AuthDataAccess.GetEmployeeByIdAsync(
            _connectionFactory,
            request.EmployeeId,
            cancellationToken);

        if (employee is null)
            return null;

        var permissions = await AuthDataAccess.GetPermissionsAsync(
            _connectionFactory,
            employee,
            cancellationToken);

        return new MeResponseDto
        {
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            Username = employee.Username,
            Role = employee.Role,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            Permissions = permissions
        };
    }
}
