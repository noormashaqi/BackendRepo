using SupermarketSystem.Api.DTOs.Auth;

namespace SupermarketSystem.Api.Features.Auth.Login;

/// <summary>
/// نتيجة عملية تسجيل الدخول. بيانات الدخول الخاطئة حالة متوقعة (مش استثناء) فبنرجعها هيك.
/// </summary>
public class LoginResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginResponseDto? Data { get; set; }

    public static LoginResult Fail(string message) => new() { Success = false, ErrorMessage = message };

    public static LoginResult Ok(LoginResponseDto data) => new() { Success = true, Data = data };
}
