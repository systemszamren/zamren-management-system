using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class ProcessService : IProcessService
{
    private readonly AuthContext _context;
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(AuthContext context, ILogger<ProcessService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ICollection<WkfProcess>> FindAllAsync()
    {
        return await _context.WkfProcesses
            .Include(p => p.Module)
            .ToListAsync();
    }

    public async Task<ICollection<WkfProcess>> GetParentProcessesInModuleAsync(string moduleId)
    {
        return await _context.WkfProcesses
            .Include(p => p.Module)
            .Where(p => p.ModuleId == moduleId && p.ParentProcessId == null)
            .ToListAsync();
    }

    public async Task<ICollection<WkfProcess>> FindByModuleCode(string moduleId)
    {
        return await _context.WkfProcesses
            .Include(p => p.Module)
            .Where(p => p.ModuleId == moduleId)
            .ToListAsync();
    }

    public async Task<WkfProcess?> FindByIdAsync(string id)
    {
        return await _context.WkfProcesses
            .Include(p => p.Module)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    //GetFirstStepIdAsync
    public async Task<WkfProcessStep?> GetFirstStepByProcessIdAsync(string processId)
    {
        return await _context.WkfProcessSteps
            .Where(s => s.ProcessId == processId && s.IsInitialStep)
            .FirstOrDefaultAsync();
    }

    public async Task<IdentityResult> CreateAsync(WkfProcess process)
    {
        try
        {
            await _context.WkfProcesses.AddAsync(process);
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

    public async Task<IdentityResult> UpdateAsync(WkfProcess process)
    {
        try
        {
            _context.WkfProcesses.Update(process);
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

    public async Task<IdentityResult> DeleteAsync(WkfProcess process)
    {
        try
        {
            _context.WkfProcesses.Remove(process);
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

    public Task<WkfProcess?> FindByNameAsync(string name)
    {
        return _context.WkfProcesses.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<bool> ProcessNameExistsExceptAsync(string name, string processId)
    {
        return await _context.WkfProcesses.AnyAsync(p => p.Name == name && p.Id != processId);
    }

    public async Task<ICollection<WkfProcessStep>> FindStepsAsync(string processId)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Process)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .Include(s => s.Privilege)
            .Where(s => s.ProcessId == processId)
            .ToListAsync();
    }

    public async Task<ICollection<WkfProcess>> FindChildProcessesAsync(string processId)
    {
        return await _context.WkfProcesses
            .Include(p => p.Module)
            .Where(p => p.ParentProcessId == processId)
            .ToListAsync();
    }

    public async Task<int> CountStepsAsync(string processId)
    {
        return await _context.WkfProcessSteps.CountAsync(s => s.ProcessId == processId);
    }
}