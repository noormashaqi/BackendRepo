using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupermarketSystem.Api.Services.Permissions;

namespace SupermarketSystem.Api.Common;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class PermissionRequirementAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionKey;

    public PermissionRequirementAttribute(string permissionKey)
    {
        _permissionKey = permissionKey;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var employeeIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? user.FindFirst("sub");

        if (employeeIdClaim is null || !long.TryParse(employeeIdClaim.Value, out var employeeId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        var hasPermission = await permissionService.HasPermissionAsync(employeeId, _permissionKey);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}