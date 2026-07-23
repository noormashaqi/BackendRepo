namespace SupermarketSystem.Api.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public long EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = new();
}
