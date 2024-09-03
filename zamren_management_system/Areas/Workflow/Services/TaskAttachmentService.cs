using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class TaskAttachmentService : ITaskAttachmentService
{
    private readonly AuthContext _context;
    private readonly ILogger<TaskAttachmentService> _logger;

    public TaskAttachmentService(AuthContext context, ILogger<TaskAttachmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    //create
    public async Task<IdentityResult> CreateAsync(WkfTaskAttachment taskAttachment)
    {
        try
        {
            await _context.WkfTaskAttachments.AddAsync(taskAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateTaskAttachmentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }
    
    public async Task<IdentityResult> CreateAsync(IEnumerable<WkfTaskAttachment> taskAttachments)
    {
        try
        {
            _context.WkfTaskAttachments.AddRange(taskAttachments);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateTaskAttachmentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //update
    public async Task<IdentityResult> UpdateAsync(WkfTaskAttachment taskAttachment)
    {
        try
        {
            _context.WkfTaskAttachments.Update(taskAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateTaskAttachmentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //delete
    public async Task<IdentityResult> DeleteAsync(WkfTaskAttachment taskAttachment)
    {
        try
        {
            _context.WkfTaskAttachments.Remove(taskAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteTaskAttachmentAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //find by id
    public async Task<WkfTaskAttachment?> FindByIdAsync(string id)
    {
        return await _context.WkfTaskAttachments
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}