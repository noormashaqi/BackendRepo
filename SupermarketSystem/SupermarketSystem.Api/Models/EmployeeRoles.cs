namespace SupermarketSystem.Api.Models;

public static class EmployeeRoles
{
    public const string Admin = "Admin";
    public const string Cashier = "Cashier";
    public const string InventoryEmployee = "InventoryEmployee";

    public static bool IsValid(string role)
    {
        return role == Admin
            || role == Cashier
            || role == InventoryEmployee;
    }
}