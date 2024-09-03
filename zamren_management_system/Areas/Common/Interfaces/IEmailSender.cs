using zamren_management_system.Areas.Common.Models;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface IEmailSender
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="recipient">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body of the email.</param>
    /// <param name="systemAttachments">The list of attachments to include in the email.</param>
    /// <param name="currentUserId"></param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    Task<bool> SendEmailAsync(string recipient, string subject, string body,
        List<SystemAttachment>? systemAttachments, string? currentUserId);

    /// <summary>
    ///  Sends an email.
    /// </summary>
    /// <param name="recipient"> The recipient's email address. </param>
    ///  <param name="subject"> The subject of the email. </param>
    ///  <param name="body"> The body of the email. </param>
    /// <param name="currentUserId"> The current user's ID. </param>
    /// <returns> True if the email was sent successfully, false otherwise. </returns>
    Task<bool> SendEmailAsync(string recipient, string subject, string body, string? currentUserId);

    /// <summary>
    ///     Sends an email with.
    /// </summary>
    /// <param name="recipient"> The recipient's email address. </param>
    /// <param name="ccEmails"> The list of CC recipient's email addresses. </param>
    /// <param name="subject"> The subject of the email. </param>
    /// <param name="body"> The body of the email. </param>
    /// <param name="currentUserId"> The current user's ID. </param>
    /// <returns> True if the email was sent successfully, false otherwise. </returns>
    Task<bool> SendEmailAsync(string recipient, List<string> ccEmails, string subject, string body,
        string? currentUserId);
}