namespace SupermarketSystem.Api.Services.Permissions;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        long employeeId,
        string permissionKey,
        CancellationToken cancellationToken = default);
}