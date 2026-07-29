namespace SupermarketSystem.Api.DTOs.Reports;

public class AttendanceReportDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public long? EmployeeId { get; set; }

    public int EntryCount { get; set; }

    public IReadOnlyCollection<AttendanceReportEntryDto> Entries { get; set; } = Array.Empty<AttendanceReportEntryDto>();
}

public class AttendanceReportEntryDto
{
    public long AttendanceLogId { get; set; }

    public long EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public double? ShiftDurationHours { get; set; }
}
