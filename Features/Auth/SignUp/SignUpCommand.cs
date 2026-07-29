using MediatR;
using SupermarketSystem.Api.DTOs.Auth;

namespace SupermarketSystem.Api.Features.Auth.SignUp;

public class SignUpCommand : IRequest<SignUpResult>
{
    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Cashier";
}

public class SignUpResult
{
    public bool Success { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginResponseDto? Data { get; set; }

    public static SignUpResult Ok(LoginResponseDto data) => new() { Success = true, Data = data };

    public static SignUpResult Fail(string errorCode, string message) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        ErrorMessage = message
    };
}
