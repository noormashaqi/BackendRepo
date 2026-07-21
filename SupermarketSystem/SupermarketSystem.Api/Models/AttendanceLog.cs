public class AttendanceLog
{
    public long Id {get;set;}

    public long EmployeeId {get;set;}

    public DateTime LoginTime {get;set;}

    public DateTime? LogoutTime {get;set;}

    public required Employee Employee {get;set;}
}