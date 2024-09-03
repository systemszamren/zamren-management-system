using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Models;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface ISystemNotificationService
{
    Task<IdentityResult> CreateAsync(SystemNotification systemNotification);

    Task<IdentityResult> CreateAsync(IEnumerable<SystemNotification> systemNotifications);

    Task<IdentityResult> UpdateAsync(SystemNotification systemNotification);

    Task<IdentityResult> UpdateAsync(IEnumerable<SystemNotification> systemNotifications);

    Task<IdentityResult> DeleteAsync(SystemNotification systemNotification);

    Task<IdentityResult> DeleteAsync(IEnumerable<SystemNotification> systemNotifications);

    Task<SystemNotification?> FindByIdAsync(string id);

    Task<IEnumerable<SystemNotification>> FindByUserIdAsync(string userId);

    Task<IEnumerable<SystemNotification>> FindAllAsync();
}