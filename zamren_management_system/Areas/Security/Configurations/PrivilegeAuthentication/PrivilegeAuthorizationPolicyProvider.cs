using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;

public class PrivilegeAuthorizationPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public PrivilegeAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
            return policy;

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PrivilegeRequirement(policyName))
            .Build();
    }
}