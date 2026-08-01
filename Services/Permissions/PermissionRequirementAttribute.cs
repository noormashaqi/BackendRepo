using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SupermarketSystem.Api.Services.Permissions;

public class PermissionRequirementAttribute 
    : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionKey;

    public PermissionRequirementAttribute(string permissionKey)
    {
        _permissionKey = permissionKey;
    }


    public async Task OnAuthorizationAsync(
        AuthorizationFilterContext context)
    {
        var permissionService =
            context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();


        var employeeIdClaim =
            context.HttpContext.User
            .FindFirst("sub");


        if (employeeIdClaim is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }


        var employeeId =
            long.Parse(employeeIdClaim.Value);


        var hasPermission =
            await permissionService.HasPermissionAsync(
                employeeId,
                _permissionKey);


        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}