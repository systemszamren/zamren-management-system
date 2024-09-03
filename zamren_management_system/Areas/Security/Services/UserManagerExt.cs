using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Security.Services;

public static class UserManagerExt
{
    /// <summary>
    /// This method is used to schedule account deletion by setting the user's status to ScheduledForDeletion, account deletion scheduled date and is scheduled for deletion to true
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="modifiedByUserId"></param>
    /// <param name="daysToDeletion"></param>
    /// <returns> IdentityResult </returns>
    public static async Task<IdentityResult> ScheduleAccountDeletionAsync(this UserManager<ApplicationUser> userManager,
        ApplicationUser user, string? modifiedByUserId, double daysToDeletion)
    {
        user.RecentActivity = Status.ScheduledForDeletion.ToString();
        user.AccountDeletionScheduledDate = DateTimeOffset.UtcNow.AddDays(daysToDeletion);
        user.IsScheduledForDeletion = true;
        user.ModifiedByUserId = modifiedByUserId;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        return IdentityResult.Success;
    }

    /// <summary>
    /// This method is used to reverse account deletion by setting the user's status to ReversedDeletion, is scheduled for deletion to false and account deletion scheduled date to null
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="modifiedByUserId"></param>
    /// <returns> IdentityResult </returns>
    public static async Task<IdentityResult> ReverseAccountDeletionAsync(this UserManager<ApplicationUser> userManager,
        ApplicationUser user, string? modifiedByUserId)
    {
        user.RecentActivity = Status.ReversedDeletion.ToString();
        user.IsScheduledForDeletion = false;
        user.AccountDeletionScheduledDate = null;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        user.ModifiedByUserId = modifiedByUserId;
        await userManager.UpdateAsync(user);
        return IdentityResult.Success;
    }

    /// <summary>
    /// This method is used to lock a user's account by setting the user's lockout end to DateTimeOffset.MaxValue, lockout enabled to true, status to locked, modified by user id and modified date
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="modifiedByUserId"></param>
    /// <returns> IdentityResult </returns>
    public static async Task<IdentityResult> LockAccountAsync(this UserManager<ApplicationUser> userManager,
        ApplicationUser user, string? modifiedByUserId)
    {
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.LockoutEnabled = true;
        user.RecentActivity = Status.Locked.ToString();
        user.ModifiedByUserId = modifiedByUserId;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        return IdentityResult.Success;
    }

    /// <summary>
    /// This method is used to unlock a user's account by setting the user's lockout end to null, lockout enabled to false, status to unlocked, modified by user id and modified date
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="modifiedByUserId"></param>
    /// <returns> IdentityResult </returns>
    public static async Task<IdentityResult> UnlockAccountAsync(this UserManager<ApplicationUser> userManager,
        ApplicationUser user, string? modifiedByUserId)
    {
        user.LockoutEnd = null;
        user.LockoutEnabled = false;
        user.RecentActivity = Status.AccountUnlocked.ToString();
        user.ModifiedByUserId = modifiedByUserId;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        return IdentityResult.Success;
    }

    /// <summary>
    ///  This method is used to update a user's recent activity by setting the user's recent activity
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="modifiedByUserId"></param>
    /// <param name="recentActivity"></param>
    /// <returns></returns>
    public static async Task<IdentityResult> UpdateRecentActivityAsync(this UserManager<ApplicationUser> userManager,
        ApplicationUser user, string? modifiedByUserId, string recentActivity)
    {
        user.RecentActivity = recentActivity;
        user.ModifiedByUserId = modifiedByUserId;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        return IdentityResult.Success;
    }

    /// <summary>
    /// This method is used to confirm a user's account email by setting the user's email confirmed to true, status to email confirmed, modified by user id and modified date
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="modifiedByUserId"></param>
    /// <returns> IdentityResult </returns>
    public static async Task ConfirmAccountEmailAsync(this UserManager<ApplicationUser> userManager,
        ApplicationUser user, string? modifiedByUserId)
    {
        user.EmailConfirmed = true;
        user.RecentActivity = Status.EmailConfirmed.ToString();
        user.ModifiedByUserId = modifiedByUserId;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
    }
}