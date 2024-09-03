using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Security.Services;

public class RoleService : IRoleService
{
    private readonly AuthContext _context;
    private readonly ILogger<RoleService> _logger;

    public RoleService(AuthContext context, ILogger<RoleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasPrivilegeAsync(string roleId, string privilegeId)
    {
        try
        {
            return await _context.RolePrivileges
                .AnyAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == privilegeId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return false;
        }
    }
}