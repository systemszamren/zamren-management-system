using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class TaskService : ITaskService
{
    private readonly AuthContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AuthContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(WkfTask task)
    {
        try
        {
            await _context.WkfTasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateTaskAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(WkfTask task)
    {
        try
        {
            _context.WkfTasks.Update(task);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateTaskAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(WkfTask task)
    {
        try
        {
            _context.WkfTasks.Remove(task);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteTaskAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<WkfTask?> FindByIdAsync(string id)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    /*using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfTask
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string Reference { get; set; }

    public string ParentTaskId { get; set; } //only updated at initiation
    public WkfTask? ParentTask { get; set; }

    [Required] public string CurrentProcessId { get; set; }
    public WkfProcess CurrentProcess { get; set; }

    [Required] public DateTimeOffset CurrentProcessStartDate { get; set; }
    public DateTimeOffset? CurrentProcessEndDate { get; set; }

    public string? CurrentStepId { get; set; }
    public WkfProcessStep? CurrentStep { get; set; }

    [Required] public DateTimeOffset CurrentStepStartedDate { get; set; }

    public DateTimeOffset? CurrentStepExpectedEndDate { get; set; }

    [Required] public string TaskStartedByUserId { get; set; }
    public ApplicationUser TaskStartedByUser { get; set; }

    public string? CurrentActioningUserId { get; set; }
    public ApplicationUser? CurrentActioningUser { get; set; }
    
    public string? PreviousActioningUserId { get; set; }
    public ApplicationUser? PreviousActioningUser { get; set; }

    [Required] public bool IsOpen { get; set; }
    [Required] public bool WasSentBack { get; set; }
    
    public string? SentBackAtStepId { get; set; }
    public WkfProcessStep? SentBackAtStep { get; set; }

    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    // public IEnumerable<WkfTaskLog>? WkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskComment>? WkfTaskComments { get; set; }
    public IEnumerable<WkfTaskAttachment>? WkfTaskAttachments { get; set; }
    public IEnumerable<WkfTask>? ChildTasks { get; set; }
    public IEnumerable<WkfTaskLog>? TaskWkfTaskLogs { get; set; }
}*/

    public async Task<WkfTask?> FindByReferenceAsync(string reference)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Include(t => t.WkfTaskComments)
            .ThenInclude(c => c.WkfTaskAttachments)
            // .Include(t => t.WkfTaskAttachments)
            .FirstOrDefaultAsync(t => t.Reference == reference);
    }

    public async Task<List<WkfTask>> FindByProcessIdAsync(string processId)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Where(t => t.CurrentProcessId == processId)
            .ToListAsync();
    }

    public async Task<List<WkfTask>> FindByCurrentStepIdAsync(string stepId)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Where(t => t.CurrentStepId == stepId)
            .ToListAsync();
    }

    public async Task<List<WkfTask>> FindByTaskStartedByUserIdAsync(string userId)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Where(t => t.TaskStartedByUserId == userId)
            .ToListAsync();
    }

    public async Task<List<WkfTask>> FindByCurrentActioningUserIdAsync(string userId)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Where(t => t.CurrentActioningUserId == userId)
            .ToListAsync();
    }

    public async Task<List<WkfTask>> FindByUserIdAsync(string userId)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Where(t => t.CurrentActioningUserId == userId)
            .ToListAsync();
    }

    public async Task<List<WkfTask>> FindAllAsync()
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .ToListAsync();
    }
    
    public async Task<List<WkfTask>> GetChildTasksAsync(string taskId)
    {
        return await _context.WkfTasks
            .Include(t => t.CurrentProcess)
            .ThenInclude(p => p.Module)
            .Include(t => t.CurrentStep)
            .ThenInclude(ns => ns.NextStep)
            .ThenInclude(ps => ps.PreviousStep)
            .Include(t => t.TaskStartedByUser)
            .Include(t => t.CurrentActioningUser)
            .Where(t => t.ParentTaskId == taskId)
            .ToListAsync();
    }
}