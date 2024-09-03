using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class ModuleService : IModuleService
{
    private readonly AuthContext _context;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(AuthContext context, ILogger<ModuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ICollection<Module>> FindAllAsync()
    {
        return await _context.Modules.ToListAsync();
    }

    public async Task<IEnumerable<ModulePrivilege?>> FindPrivilegesAsync(string moduleId)
    {
        return await _context.ModulePrivileges
            .Include(mp => mp.Privilege)
            .Include(mp => mp.Module)
            .Where(mp => mp.ModuleId == moduleId)
            .ToListAsync();
    }

    public async Task<bool> ModuleNameExistsAsync(string name)
    {
        return await _context.Modules.AnyAsync(p => p.Name == name);
    }

    public async Task<bool> ModuleNameExistsExceptAsync(string name, string moduleId)
    {
        return await _context.Modules.AnyAsync(p => p.Name == name && p.Id != moduleId);
    }

    public async Task<bool> ModuleCodeExistsAsync(string code)
    {
        return await _context.Modules.AnyAsync(p => p.Code == code);
    }

    public async Task<bool> ModuleCodeExistsExceptAsync(string code, string moduleId)
    {
        return await _context.Modules.AnyAsync(p => p.Code == code && p.Id != moduleId);
    }

    public async Task<IdentityResult> CreateAsync(Module module)
    {
        try
        {
            await _context.Modules.AddAsync(module);
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

    public async Task<IdentityResult> CreateAsync(IEnumerable<Module> modules)
    {
        try
        {
            _context.Modules.AddRange(modules);
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

    public async Task<IdentityResult> RemovePrivilegeAsync(string moduleId, string privilegeId)
    {
        try
        {
            var modulePrivilege = await _context.ModulePrivileges
                .FirstOrDefaultAsync(mp => mp.PrivilegeId == privilegeId && mp.ModuleId == moduleId);
            if (modulePrivilege == null)
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "RemovePrivilegeAsync",
                    Description = "Privilege not found"
                });

            _context.ModulePrivileges.Remove(modulePrivilege);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RemovePrivilegeAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public Task<List<Module>> FindAllExceptAsync(string moduleId)
    {
        return _context.Modules.Where(m => m.Id != moduleId).ToListAsync();
    }

    public async Task<IdentityResult> AddPrivilegeAsync(ModulePrivilege modulePrivilege)
    {
        try
        {
            await _context.ModulePrivileges.AddAsync(modulePrivilege);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "AddPrivilegeAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<Module?> FindByCodeAsync(string code)
    {
        return await _context.Modules.FirstOrDefaultAsync(m => m.Code == code);
    }

    public async Task<Module?> FindByIdAsync(string moduleId)
    {
        return await _context.Modules.FindAsync(moduleId);
    }

    public async Task<IEnumerable<Privilege>> FindPrivilegesNotAssignedToModuleAsync(string moduleId)
    {
        return await _context.ModulePrivileges
            .Where(mp => mp.ModuleId != moduleId)
            .Select(mp => mp.Privilege)
            .ToListAsync();
    }

    public async Task<IdentityResult> UpdateAsync(Module module)
    {
        try
        {
            _context.Modules.Update(module);
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

    public async Task<IdentityResult> DeleteAsync(Module module)
    {
        try
        {
            _context.Modules.Remove(module);
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

    public async Task<bool> IsPrivilegeAssignedToModuleAsync(string privilegeId, string moduleId)
    {
        return await _context.ModulePrivileges
            .AnyAsync(mp => mp.PrivilegeId == privilegeId && mp.ModuleId == moduleId);
    }

    public async Task<IEnumerable<Privilege>> FindNotInModuleAsync(string moduleId)
    {
        return await _context.Privileges
            .Where(p => !_context.ModulePrivileges.Any(mp => mp.PrivilegeId == p.Id && mp.ModuleId == moduleId))
            .ToListAsync();
    }

    public async Task<bool> IsInModuleAsync(string privilegeId, string moduleId)
    {
        return await _context.ModulePrivileges
            .AnyAsync(mp => mp.PrivilegeId == privilegeId && mp.ModuleId == moduleId);
    }
}