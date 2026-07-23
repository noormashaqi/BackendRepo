namespace SupermarketSystem.Api.Dtos.Permissions;

public class EmployeePermissionsResponse
{
    public long EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Permissions { get; set; }
        = Array.Empty<string>();
}