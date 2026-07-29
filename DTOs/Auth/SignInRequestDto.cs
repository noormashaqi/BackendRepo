namespace SupermarketSystem.Api.DTOs.Auth;

public class SignInRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
