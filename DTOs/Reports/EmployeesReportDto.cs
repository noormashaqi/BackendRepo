namespace SupermarketSystem.Api.DTOs.Reports;

public class EmployeesReportDto
{
    public bool? ActiveOnly { get; set; }

    public string? Role { get; set; }

    public int EmployeeCount { get; set; }

    public IReadOnlyCollection<EmployeeReportItemDto> Employees { get; set; } = Array.Empty<EmployeeReportItemDto>();
}

public class EmployeeReportItemDto
{
    public long EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int PermissionCount { get; set; }
}
