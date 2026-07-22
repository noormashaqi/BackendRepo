namespace SupermarketSystem.Api.Dtos.Permissions;

public class UpdateEmployeePermissionsRequest
{
    public List<string> PermissionsToAdd { get; set; } = [];

    public List<string> PermissionsToRemove { get; set; } = [];
}