using Microsoft.AspNetCore.Authorization;

namespace zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;

public class PrivilegeRequirement: IAuthorizationRequirement
{
    public PrivilegeRequirement(string privilege)
    {
        Privilege = privilege;
    }

    public string Privilege { get; }
}