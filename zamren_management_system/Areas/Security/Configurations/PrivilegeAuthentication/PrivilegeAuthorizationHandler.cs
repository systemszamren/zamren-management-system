using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;

public class PrivilegeAuthorizationHandler
    : AuthorizationHandler<PrivilegeRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PrivilegeAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PrivilegeRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        using var scope = _serviceScopeFactory.CreateScope();
        var iPrivilegeService = scope.ServiceProvider
            .GetRequiredService<IPrivilegeService>();

        var privileges = await iPrivilegeService.FindNamesByUserAsync(userId!);

        var requiredPrivileges = requirement.Privilege.Split(',');

        if (requiredPrivileges.Any(requiredPrivilege => privileges.Contains(requiredPrivilege)))
        {
            context.Succeed(requirement);
            return;
        }
    }
}