using MediatR;

namespace SupermarketSystem.Api.Features.Auth.Logout;

public class LogoutCommand : IRequest<LogoutResult>
{
    public long EmployeeId { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public static LogoutResult Ok() => new() { Success = true };

    public static LogoutResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
