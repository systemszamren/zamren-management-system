using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Services;

public class SystemAttachmentService : ISystemAttachmentService
{
    private readonly AuthContext _context;
    private readonly ILogger<SystemAttachmentService> _logger;

    public SystemAttachmentService(AuthContext context, ILogger<SystemAttachmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(SystemAttachment systemAttachment)
    {
        try
        {
            await _context.SystemAttachments.AddAsync(systemAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save attachment"
            });
        }
    }

    public async Task<IdentityResult> CreateAsync(IEnumerable<SystemAttachment> systemAttachments)
    {
        try
        {
            _context.SystemAttachments.AddRange(systemAttachments);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save attachments"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(SystemAttachment systemAttachment)
    {
        try
        {
            _context.SystemAttachments.Update(systemAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "Failed to update attachment"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(SystemAttachment systemAttachment)
    {
        try
        {
            _context.SystemAttachments.Remove(systemAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "Failed to delete attachment"
            });
        }
    }

    public async Task<SystemAttachment?> FindByIdAsync(string id)
    {
        return await _context.SystemAttachments.FindAsync(id);
    }

    //FindAll
    public async Task<IEnumerable<SystemAttachment>> FindAllAsync()
    {
        return await _context.SystemAttachments.ToListAsync();
    }

    public async Task<IEnumerable<SystemAttachment>> GetByUserIdAsync(string userId)
    {
        return await _context.SystemAttachments.Where(a => a.UploadedByUserId == userId).ToListAsync();
    }
}