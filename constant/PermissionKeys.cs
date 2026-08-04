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

    // Categories
    public const string CategoriesView = "categories.view";
    public const string CategoriesCreate = "categories.create";

    // Products
    public const string ProductsView = "products.view";
    public const string ProductsCreate = "products.create";
    public const string ProductsUpdate = "products.update";
    public const string ProductsDeactivate = "products.deactivate";
    public const string ProductsStockAdd = "products.stock_add";

    // Invoices
    public const string InvoicesCreate = "invoices.create";
    public const string InvoicesView = "invoices.view";

    // Reports
    public const string ReportsView = "reports.view";


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
        SalesView,

        CategoriesView,
        CategoriesCreate,

        ProductsView,
        ProductsCreate,
        ProductsUpdate,
        ProductsDeactivate,
        ProductsStockAdd,

        InvoicesCreate,
        InvoicesView,
        
        ReportsView
    ];


    public static bool IsValid(string permissionKey)
    {
        return All.Contains(
            permissionKey,
            StringComparer.OrdinalIgnoreCase);
    }
}