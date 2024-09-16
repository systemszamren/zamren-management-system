using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class PrivilegeService : IPrivilegeService
{
    private readonly AuthContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<PrivilegeService> _logger;

    public PrivilegeService(AuthContext context,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        ILogger<PrivilegeService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<HashSet<string>> FindNamesByUserAsync(string userId)
    {
        try
        {
            //get user by userId
            var user = await _userManager.FindByIdAsync(userId);

            //get user roles
            var roleNames = await _userManager.GetRolesAsync(user!);

            //look through the user roles and get the role details
            var roles = new List<ApplicationRole>();
            foreach (var r in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(r);
                if (role != null) roles.Add(role);
            }

            //get role Ids
            var roleIds = roles.Select(r => r.Id).ToList();

            //get privileges from RolePrivilege table by roleIds
            var privilegesIds = _context.RolePrivileges
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PrivilegeId).ToList();

            //get privileges from Privilege table by privilegeIds
            var privilegesNames = await _context.Privileges
                .Where(p => privilegesIds.Contains(p.Id))
                .Select(p => p.Name).ToListAsync();

            return privilegesNames.ToHashSet();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new HashSet<string>();
        }
    }

    public async Task<bool> PrivilegeNameExistsAsync(string name)
    {
        return await _context.Privileges.AnyAsync(p => p.Name == name);
    }

    //Checks if record exists except for the record with the given ID
    public async Task<bool> PrivilegeNameExistsExceptAsync(string name, string id)
    {
        return await _context.Privileges.AnyAsync(p => p.Name == name && p.Id != id);
    }

    public async Task<IdentityResult> CreateAsync(Privilege privilege)
    {
        try
        {
            await _context.Privileges.AddAsync(privilege);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(string roleId, string privilegeId)
    {
        try
        {
            var rolePrivilege = await _context.RolePrivileges
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == privilegeId);

            if (rolePrivilege == null) return IdentityResult.Success;
            _context.RolePrivileges.Remove(rolePrivilege);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RemoveFromRoleAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> AddToRoleAsync(RolePrivilege privilegeRole)
    {
        try
        {
            await _context.RolePrivileges.AddAsync(privilegeRole);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "AddToRoleAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IEnumerable<Privilege?>> FindNotInRoleAsync(string roleId)
    {
        return await _context.Privileges
            .Where(p => !_context.RolePrivileges.Any(rp => rp.PrivilegeId == p.Id && rp.RoleId == roleId))
            .ToListAsync();
    }

    public async Task<ICollection<Privilege>> FindAllAsync()
    {
        return await _context.Privileges
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Privilege>> GetAllModulePrivilegeAsync()
    {
        return await _context.Privileges
            .Include(p => p.ModulePrivileges)
            .ThenInclude(p => p.Module)
            .OrderBy(p => p.ModulePrivileges.Select(mp => mp.Module.Name))
            .ToListAsync();
    }

    //find privileges in a module
    public async Task<ICollection<Privilege>> FindInModuleAsync(string moduleId)
    {
        return await _context.ModulePrivileges
            .Where(mp => mp.ModuleId == moduleId)
            .Select(mp => mp.Privilege).ToListAsync();
    }

    //find privileges by module name
    public async Task<ICollection<Privilege>> FindByModuleNameAsync(string moduleName)
    {
        return await _context.ModulePrivileges
            .Include(mp => mp.Module)
            .Where(mp => mp.Module.Name == moduleName)
            .Select(mp => mp.Privilege).ToListAsync();
    }

    public async Task<IEnumerable<Privilege?>> FindInRoleAsync(string roleId)
    {
        return await _context.RolePrivileges
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Privilege).ToListAsync();
    }

    public async Task<Privilege?> FindByIdAsync(string id)
    {
        return await _context.Privileges.FindAsync(id);
    }

    public async Task<IEnumerable<ApplicationRole>> FindRolesAsync(string id)
    {
        return await _context.RolePrivileges
            .Where(rp => rp.PrivilegeId == id)
            .Select(rp => rp.Role).ToListAsync();
    }

    public async Task<IdentityResult> DeleteAsync(Privilege privilege)
    {
        try
        {
            _context.Privileges.Remove(privilege);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(Privilege privilege)
    {
        try
        {
            _context.Privileges.Update(privilege);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<bool> IsInRoleAsync(string roleId, string privilegeId)
    {
        return await _context.RolePrivileges
            .AnyAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == privilegeId);
    }
}