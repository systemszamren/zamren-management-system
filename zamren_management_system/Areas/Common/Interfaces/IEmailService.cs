using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Models;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface IEmailService
{
    /// <summary>
    ///   Saves an email notification to the database.
    /// </summary>
    /// <param name="sender"> The sender's email address. </param>
    /// <param name="receiver"> The receiver's email address. </param>
    /// <param name="subject"> The subject of the email. </param>
    /// <param name="body"> The body of the email. </param>
    /// <param name="currentUserId"> The current user's ID. </param>
    /// <returns> The result of the operation. </returns>
    Task<IdentityResult> SaveEmail(string sender, string receiver, string subject,
        string body, string? currentUserId);


    /// <summary>
    ///   Saves an email notification to the database.
    /// </summary>
    /// <param name="sender"> The sender's email address. </param>
    /// <param name="receiver"> The receiver's email address. </param>
    /// <param name="subject"> The subject of the email. </param>
    /// <param name="body"> The body of the email. </param>
    /// <param name="systemAttachments"> The list of attachments to include in the email. </param>
    /// <param name="currentUserId"> The current user's ID. </param>
    /// <returns> The result of the operation. </returns>
    Task<IdentityResult> SaveEmail(string sender, string receiver, string subject, string body,
        List<SystemAttachment>? systemAttachments, string? currentUserId);
}