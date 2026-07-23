using MediatR;

namespace SupermarketSystem.Api.Features.Auth.Login;

public class LoginCommand : IRequest<LoginResult>
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
