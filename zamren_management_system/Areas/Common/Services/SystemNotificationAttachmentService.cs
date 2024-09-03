using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Services;

public class SystemNotificationAttachmentService : ISystemNotificationAttachmentService
{
    private readonly AuthContext _context;
    private readonly ILogger<SystemNotificationAttachmentService> _logger;

    public SystemNotificationAttachmentService(AuthContext context, ILogger<SystemNotificationAttachmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(SystemNotificationAttachment systemNotificationAttachment)
    {
        try
        {
            await _context.SystemNotificationAttachments.AddAsync(systemNotificationAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save notification attachment"
            });
        }
    }

    public async Task<IdentityResult> CreateAsync(
        IEnumerable<SystemNotificationAttachment> systemNotificationAttachments)
    {
        try
        {
            _context.SystemNotificationAttachments.AddRange(systemNotificationAttachments);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save notification attachments"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(SystemNotificationAttachment systemNotificationAttachment)
    {
        try
        {
            _context.SystemNotificationAttachments.Update(systemNotificationAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "Failed to update notification attachment"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(SystemNotificationAttachment systemNotificationAttachment)
    {
        try
        {
            _context.SystemNotificationAttachments.Remove(systemNotificationAttachment);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "Failed to delete notification attachment"
            });
        }
    }

    public async Task<SystemNotificationAttachment?> FindByIdAsync(string id)
    {
        return await _context.SystemNotificationAttachments.FindAsync(id);
    }

    public async Task<IEnumerable<SystemNotificationAttachment>> GetByNotificationIdAsync(string notificationId)
    {
        return await _context.SystemNotificationAttachments.Where(a => a.SystemNotificationId == notificationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemNotificationAttachment>> FindAllAsync()
    {
        return await _context.SystemNotificationAttachments.ToListAsync();
    }
}