namespace SupermarketSystem.Api.Constants;

public static class PermissionKeys
{
    // Employees
    public const string EmployeesView = "employees.view";
    public const string EmployeesCreate = "employees.create";
    public const string EmployeesUpdate = "employees.update";
    public const string EmployeesDeactivate = "employees.deactivate";
    public const string EmployeesManagePermissions =
        "employees.manage_permissions";


    // Attendance
    public const string AttendanceView = "attendance.view";
    public const string AttendanceViewEmployee =
        "attendance.view_employee";


    // Sales
    public const string SalesCreate = "sales.create";
    public const string SalesView = "sales.view";


    public static readonly IReadOnlyCollection<string> All =
    [
        EmployeesView,
        EmployeesCreate,
        EmployeesUpdate,
        EmployeesDeactivate,
        EmployeesManagePermissions,

        AttendanceView,
        AttendanceViewEmployee,

        SalesCreate,
        SalesView
    ];


    public static bool IsValid(string permissionKey)
    {
        return All.Contains(
            permissionKey,
            StringComparer.OrdinalIgnoreCase);
    }
}