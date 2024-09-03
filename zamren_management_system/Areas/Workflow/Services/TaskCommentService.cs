using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class TaskCommentService : ITaskCommentService
{
    private readonly AuthContext _context;
    private readonly ILogger<TaskCommentService> _logger;

    public TaskCommentService(AuthContext context, ILogger<TaskCommentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    //create task comment
    public async Task<IdentityResult> CreateAsync(WkfTaskComment taskComment)
    {
        try
        {
            await _context.WkfTaskComments.AddAsync(taskComment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateTaskCommentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //update task comment
    public async Task<IdentityResult> UpdateAsync(WkfTaskComment taskComment)
    {
        try
        {
            _context.WkfTaskComments.Update(taskComment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateTaskCommentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //delete task comment
    public async Task<IdentityResult> DeleteAsync(WkfTaskComment taskComment)
    {
        try
        {
            _context.WkfTaskComments.Remove(taskComment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteTaskCommentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //find task comment by id
    public async Task<WkfTaskComment?> FindByIdAsync(string id)
    {
        return await _context.WkfTaskComments
            .Include(t => t.CommentedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}