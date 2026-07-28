using MediatR;

namespace SupermarketSystem.Api.Features.Auth.ResetPassword;

public class ResetPasswordCommand : IRequest<ResetPasswordResult>
{
    public long EmployeeId { get; set; }

    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public static ResetPasswordResult Ok() => new() { Success = true };

    public static ResetPasswordResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
