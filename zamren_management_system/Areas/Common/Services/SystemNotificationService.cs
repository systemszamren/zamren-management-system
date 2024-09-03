using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Services;

public class SystemNotificationService : ISystemNotificationService
{
    private readonly AuthContext _context;
    private readonly ILogger<SystemNotificationService> _logger;

    public SystemNotificationService(AuthContext context, ILogger<SystemNotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(SystemNotification systemNotification)
    {
        try
        {
            await _context.SystemNotifications.AddAsync(systemNotification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save notification"
            });
        }
    }

    public async Task<IdentityResult> CreateAsync(IEnumerable<SystemNotification> systemNotifications)
    {
        try
        {
            _context.SystemNotifications.AddRange(systemNotifications);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save notifications"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(SystemNotification systemNotification)
    {
        try
        {
            _context.SystemNotifications.Update(systemNotification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "Failed to update notification"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(IEnumerable<SystemNotification> systemNotifications)
    {
        try
        {
            _context.SystemNotifications.UpdateRange(systemNotifications);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "Failed to update notifications"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(SystemNotification systemNotification)
    {
        try
        {
            _context.SystemNotifications.Remove(systemNotification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "Failed to delete notification"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(IEnumerable<SystemNotification> systemNotifications)
    {
        try
        {
            _context.SystemNotifications.RemoveRange(systemNotifications);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "Failed to delete notifications"
            });
        }
    }

    public async Task<SystemNotification?> FindByIdAsync(string id)
    {
        return await _context.SystemNotifications.FindAsync(id);
    }

    public async Task<IEnumerable<SystemNotification>> FindAllAsync()
    {
        //order by date posted then by priority
        return await _context.SystemNotifications
            .OrderByDescending(n => n.DatePosted)
            .ThenBy(n => n.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemNotification>> FindByUserIdAsync(string userId)
    {
        return await _context.SystemNotifications.Where(n => n.RecipientUserId == userId).ToListAsync();
    }
}