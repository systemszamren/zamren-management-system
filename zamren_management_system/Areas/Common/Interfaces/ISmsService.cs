using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Dto;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface ISmsService
{
    /// <summary>
    ///     Saves an SMS notification to the database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="recipientPhoneNumber"></param>
    /// <param name="message"></param>
    /// <param name="currentUserId"></param>
    /// <param name="smsResponseDto"></param>
    /// <returns></returns>
    Task<IdentityResult> SaveSms(string sender, string recipientPhoneNumber, string message,
        string? currentUserId, SmsResponseDto smsResponseDto);

    /// <summary>
    ///     Determines how often an SMS message can be sent to a phone number within a given time frame.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <returns></returns>
    public bool ShouldThrottle(string phoneNumber);
}