using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Services;

public class EmailService : IEmailService
{
    private readonly AuthContext _context;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(AuthContext context, ILogger<EmailService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    private async Task<IdentityResult> CreateAsync(EmailNotification emailNotification)
    {
        try
        {
            await _context.EmailNotifications.AddAsync(emailNotification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateEmailAsync",
                Description = "Failed to save email notification"
            });
        }
    }

    private async Task<IdentityResult> CreateAttachmentsAsync(
        IEnumerable<EmailNotificationAttachment> emailNotificationAttachments)
    {
        try
        {
            _context.EmailNotificationAttachments.AddRange(emailNotificationAttachments);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
        catch (Exception)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateEmailAttachmentsAsync",
                Description = "Failed to save email notification attachments"
            });
        }
    }

    public async Task<IdentityResult> SaveEmail(string sender, string receiver, string subject,
        string body, string? currentUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(subject) ||
                string.IsNullOrEmpty(body))
                throw new Exception();

            if (string.IsNullOrEmpty(currentUserId))
                currentUserId = _configuration["SystemAdminAccount:UserId"] ?? string.Empty;

            var currentDateTime = DateTimeOffset.UtcNow;
            var emailNotification = new EmailNotification
            {
                Subject = subject,
                Body = body,
                RecipientEmail = receiver,
                SenderEmail = sender,
                IsSent = true,
                DateSent = currentDateTime,
                CreatedByUserId = currentUserId,
                CreatedDate = currentDateTime
            };

            return await CreateAsync(emailNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving email notification");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save email notification"
            });
        }
    }

    public async Task<IdentityResult> SaveEmail(string sender, string receiver, string subject,
        string body, List<SystemAttachment>? systemAttachments, string? currentUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(subject) ||
                string.IsNullOrEmpty(body))
                throw new Exception();

            if (string.IsNullOrEmpty(currentUserId))
                currentUserId = _configuration["SystemAdminAccount:UserId"] ?? string.Empty;

            if (systemAttachments == null)
                return await SaveEmail(sender, receiver, subject, body, currentUserId);

            var currentDateTime = DateTimeOffset.UtcNow;
            var emailNotification = new EmailNotification
            {
                Subject = subject,
                Body = body,
                RecipientEmail = receiver,
                SenderEmail = sender,
                IsSent = true,
                DateSent = currentDateTime,
                CreatedByUserId = currentUserId,
                CreatedDate = currentDateTime
            };

            await CreateAsync(emailNotification);
            return await SaveEmailAttachments(systemAttachments, emailNotification, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving email notification");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save email notification"
            });
        }
    }

    private async Task<IdentityResult> SaveEmailAttachments(IEnumerable<SystemAttachment> systemAttachments,
        EmailNotification emailNotification,
        string? currentUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(currentUserId))
                currentUserId = _configuration["SystemAdminAccount:UserId"] ?? string.Empty;

            var currentDateTime = DateTimeOffset.UtcNow;

            var emailNotificationAttachments = new List<EmailNotificationAttachment>();
            foreach (var attachment in systemAttachments)
            {
                var emailNotificationAttachment = new EmailNotificationAttachment
                {
                    AttachmentId = attachment.Id,
                    EmailNotificationId = emailNotification.Id,
                    CreatedByUserId = currentUserId,
                    CreatedDate = currentDateTime
                };
                emailNotificationAttachments.Add(emailNotificationAttachment);
            }

            return await CreateAttachmentsAsync(emailNotificationAttachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving email notification and attachments");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "SaveEmailAttachments",
                Description = "Failed to save email notification and attachments"
            });
        }
    }
}