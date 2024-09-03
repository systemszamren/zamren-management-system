using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class TaskLogService : ITaskLogService
{
    private readonly AuthContext _context;
    private readonly ILogger<TaskLogService> _logger;

    public TaskLogService(AuthContext context, ILogger<TaskLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(WkfTaskLog taskLog)
    {
        try
        {
            await _context.WkfTaskLogs.AddAsync(taskLog);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateTaskLogAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(WkfTaskLog taskLog)
    {
        try
        {
            _context.WkfTaskLogs.Update(taskLog);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateTaskLogAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(WkfTaskLog taskLog)
    {
        try
        {
            _context.WkfTaskLogs.Remove(taskLog);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteTaskLogAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<WkfTaskLog?> FindByIdAsync(string id)
    {
        return await _context.WkfTaskLogs
            .Include(t => t.Task)
            .Include(t => t.Step)
            .Include(t => t.ActioningUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    //find task log by task id
    public async Task<List<WkfTaskLog>> FindByTaskIdAsync(string taskId)
    {
        return await _context.WkfTaskLogs
            .Include(t => t.Task)
            .Include(t => t.Step)
            .Include(t => t.ActioningUser)
            .Where(t => t.TaskId == taskId)
            .OrderByDescending(t => t.ActionDate)
            .ToListAsync();
    }

    //find task log by task id and step id
    public async Task<List<WkfTaskLog>> FindByTaskIdAndStepIdAsync(string taskId, string stepId)
    {
        return await _context.WkfTaskLogs
            .Include(t => t.Task)
            .Include(t => t.Step)
            .Include(t => t.ActioningUser)
            .Where(t => t.TaskId == taskId && t.StepId == stepId)
            .OrderByDescending(t => t.ActionDate)
            .ToListAsync();
    }
}