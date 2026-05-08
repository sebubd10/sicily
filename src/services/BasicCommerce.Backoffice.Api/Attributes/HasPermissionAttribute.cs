using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasicCommerce.Backoffice.Api.Attributes;

/// <summary>
/// Enforces fine-grained API permission check in addition to (not instead of) role policies.
/// SystemAdmin always bypasses. Users without a UserType assigned fall through to role-only auth.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _code;

    public HasPermissionAttribute(string permissionCode) => _code = permissionCode;

    public async Task OnActionExecutionAsync(ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserService>();

        if (!currentUser.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // SystemAdmin bypasses all permission checks
        if (currentUser.Role == UserRole.SystemAdmin)
        {
            await next();
            return;
        }

        // No UserType assigned — fall through (rely on [Authorize(Policy=...)] only)
        if (!currentUser.UserTypeId.HasValue)
        {
            await next();
            return;
        }

        var permService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        if (!await permService.HasPermissionAsync(currentUser.UserTypeId, _code))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
