using MediatR;
using SupermarketSystem.Api.DTOs.Auth;

namespace SupermarketSystem.Api.Features.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<RefreshTokenResult>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginResponseDto? Data { get; set; }

    public static RefreshTokenResult Ok(LoginResponseDto data) => new() { Success = true, Data = data };

    public static RefreshTokenResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
