using Microsoft.AspNetCore.Authorization;
using zamren_management_system.Areas.Security.Enums;

namespace zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;

public sealed class HasPrivilege : AuthorizeAttribute
{
    public HasPrivilege(params PrivilegeConstant[] privilegeConstants)
    {
        Policy = string.Join(",", privilegeConstants.Select(pc => pc.ToString()));
    }
}