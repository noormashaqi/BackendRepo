namespace SupermarketSystem.Api.DTOs.Auth;

public class MeResponseDto
{
    public long EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
}
