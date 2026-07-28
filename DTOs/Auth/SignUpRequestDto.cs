namespace SupermarketSystem.Api.DTOs.Auth;

public class SignUpRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Cashier";
}
