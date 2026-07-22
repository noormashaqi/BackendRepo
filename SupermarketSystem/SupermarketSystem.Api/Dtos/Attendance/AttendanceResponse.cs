namespace SupermarketSystem.Api.Dtos.Attendance;

public class AttendanceResponse
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }
}