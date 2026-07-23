using MediatR;

namespace SupermarketSystem.Api.Features.Auth.Logout;

public class LogoutCommand : IRequest<bool>
{
    public long EmployeeId { get; set; }
}
