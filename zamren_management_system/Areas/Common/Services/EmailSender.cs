using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Common.ViewModels;
using Attachment = System.Net.Mail.Attachment;

namespace zamren_management_system.Areas.Common.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailConfigurationViewModel _emailConfig;
    private readonly ILogger<EmailSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IUtil _util;

    public EmailSender(IOptions<EmailConfigurationViewModel> emailConfig, ILogger<EmailSender> logger,
        IConfiguration configuration, IEmailService emailService, IUtil util)
    {
        _emailConfig = emailConfig.Value;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
        _util = util;
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string body, string? currentUserId)
    {
        try
        {
            // If the 'To' email is empty, use the test email
            if (!string.IsNullOrEmpty(_emailConfig.To)) recipient = _emailConfig.To;
            var mailMessage = new MailMessage(_emailConfig.From, recipient)
            {
                Subject = subject,
                Body = body
            };

            // Add the backup email to keep a record of the email sent
            if (!string.IsNullOrEmpty(_emailConfig.CcBackup))
                mailMessage.CC.Add(_emailConfig.CcBackup);

            mailMessage.IsBodyHtml = true;

            var networkCred = new NetworkCredential
            {
                UserName = _emailConfig.Username,
                Password = _emailConfig.Password
            };

            var smtp = new SmtpClient
            {
                Host = _emailConfig.SmtpServer,
                EnableSsl = _emailConfig.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = networkCred,
                Port = _emailConfig.Port
            };

            var emailSent = false;
            var retryCount = 0;

            while (!emailSent && retryCount < 3) // Retry sending the email up to 3 times
            {
                try
                {
                    smtp.Send(mailMessage);
                    smtp.Dispose();
                    emailSent = true;
                }
                catch (SmtpException e)
                {
                    // Handle the "Service not available, closing transmission channel" error
                    // Sleep for a short duration before retrying
                    retryCount++;
                    _logger.LogWarning($"Error sending email to {recipient}. Retrying... Retry Count: {retryCount}");
                    _logger.LogError(e, "Failed to perform action");
                    var timeOut = Convert.ToInt32(_configuration["SystemVariables:ThreadTimeOutInMilliSec"]);
                    Thread.Sleep(timeOut);
                }
            }

            if (!emailSent)
            {
                // Log the failed email sending or handle the failure as needed
                _logger.LogError($"Failed to send email to {recipient} after retries.");
                return false;
            }

            // Save the email to the database
            await _emailService.SaveEmail(_emailConfig.From, recipient, subject, body, currentUserId);

            _logger.LogInformation($"Email sent from {_emailConfig.From} to {recipient}: " + DateTimeOffset.UtcNow);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to perform action");
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string body,
        List<SystemAttachment>? systemAttachments, string? currentUserId)
    {
        var attachments = new List<Attachment>();
        try
        {
            // If the 'To' email is empty, use the test email
            if (!string.IsNullOrEmpty(_emailConfig.To)) recipient = _emailConfig.To;
            var mailMessage = new MailMessage(_emailConfig.From, recipient)
            {
                Subject = subject,
                Body = body
            };

            // Add the backup email to keep a record of the email sent
            if (!string.IsNullOrEmpty(_emailConfig.CcBackup))
                mailMessage.CC.Add(_emailConfig.CcBackup);

            //check if attachments are not null and have any attachments
            if (systemAttachments != null && systemAttachments.Any())
            {
                foreach (var systemAttachment in systemAttachments)
                {
                    var path = systemAttachment.FilePath;
                    var wwwrootPath = _util.GetWwwRootPath();
                    if (string.IsNullOrEmpty(path)) continue;
                    var attachment = new Attachment(wwwrootPath + path);
                    attachments.Add(attachment);
                    mailMessage.Attachments.Add(attachment);
                }
            }

            mailMessage.IsBodyHtml = true;

            var networkCred = new NetworkCredential
            {
                UserName = _emailConfig.Username,
                Password = _emailConfig.Password
            };

            var smtp = new SmtpClient
            {
                Host = _emailConfig.SmtpServer,
                EnableSsl = _emailConfig.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = networkCred,
                Port = _emailConfig.Port
            };

            var emailSent = false;
            var retryCount = 0;

            while (!emailSent && retryCount < 3) // Retry sending the email up to 3 times
            {
                try
                {
                    smtp.Send(mailMessage);
                    smtp.Dispose();
                    emailSent = true;
                }
                catch (SmtpException e)
                {
                    // Handle the "Service not available, closing transmission channel" error
                    // Sleep for a short duration before retrying
                    retryCount++;
                    _logger.LogWarning($"Error sending email to {recipient}. Retrying... Retry Count: {retryCount}");
                    _logger.LogError(e, "Failed to perform action");
                    var timeOut = Convert.ToInt32(_configuration["SystemVariables:ThreadTimeOutInMilliSec"]);
                    Thread.Sleep(timeOut);
                }
            }

            if (!emailSent)
            {
                // Log the failed email sending or handle the failure as needed
                _logger.LogError($"Failed to send email to {recipient} after retries.");
                return false;
            }

            // Dispose the MemoryStreams after the email has been sent
            if (attachments.Any())
            {
                foreach (var attachment in attachments)
                    attachment.Dispose();
            }

            // Save the email to the database
            // await _emailService.SaveEmail(_emailConfig.From, recipient, subject, body, currentUserId);
            await _emailService.SaveEmail(_emailConfig.From, recipient, subject, body, systemAttachments,
                currentUserId);

            _logger.LogInformation($"Email sent from {_emailConfig.From} to {recipient}: " + DateTimeOffset.UtcNow);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to perform action");
            return false;
        }
        finally
        {
            // Dispose the MemoryStreams after the email has been sent
            if (attachments.Any())
            {
                foreach (var attachment in attachments)
                    attachment.Dispose();
            }
        }
    }

    public async Task<bool> SendEmailAsync(string recipient, List<string> ccEmails, string subject, string body,
        string? currentUserId)
    {
        try
        {
            // If the 'To' email is empty, use the test email
            if (!string.IsNullOrEmpty(_emailConfig.To)) recipient = _emailConfig.To;
            var mailMessage = new MailMessage(_emailConfig.From, recipient)
            {
                Subject = subject,
                Body = body
            };

            // Add the backup email to keep a record of the email sent
            if (!string.IsNullOrEmpty(_emailConfig.CcBackup))
                mailMessage.CC.Add(_emailConfig.CcBackup);

            foreach (var ccEmail in ccEmails.Where(ccEmail => !string.IsNullOrEmpty(ccEmail)))
            {
                mailMessage.CC.Add(ccEmail);
            }

            mailMessage.IsBodyHtml = true;

            var networkCred = new NetworkCredential
            {
                UserName = _emailConfig.Username,
                Password = _emailConfig.Password
            };

            var smtp = new SmtpClient
            {
                Host = _emailConfig.SmtpServer,
                EnableSsl = _emailConfig.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = networkCred,
                Port = _emailConfig.Port
            };

            var emailSent = false;
            var retryCount = 0;

            while (!emailSent && retryCount < 3) // Retry sending the email up to 3 times
            {
                try
                {
                    smtp.Send(mailMessage);
                    smtp.Dispose();
                    emailSent = true;
                }
                catch (SmtpException e)
                {
                    // Handle the "Service not available, closing transmission channel" error
                    // Sleep for a short duration before retrying
                    retryCount++;
                    _logger.LogWarning($"Error sending email to {recipient}. Retrying... Retry Count: {retryCount}");
                    _logger.LogError(e, "Failed to perform action");
                    var timeOut = Convert.ToInt32(_configuration["SystemVariables:ThreadTimeOutInMilliSec"]);
                    Thread.Sleep(timeOut);
                }
            }

            if (!emailSent)
            {
                // Log the failed email sending or handle the failure as needed
                _logger.LogError($"Failed to send email to {recipient} after retries.");
                return false;
            }

            // Save the email to the database
            await _emailService.SaveEmail(_emailConfig.From, recipient, subject, body, currentUserId);

            _logger.LogInformation($"Email sent from {_emailConfig.From} to {recipient}: " + DateTimeOffset.UtcNow);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to perform action");
            return false;
        }
    }


    //SENDING EMAIL WITH ATTACHMENTS****************************************************
    /*var systemAttachments = await _systemAttachmentService.FindAllAsync();
        var systemAttachmentsList = systemAttachments.ToList();

        await _emailSender.SendEmail(
            user.Email!,
            "Reset Password",
            body,
            systemAttachmentsList,
            _userManager.GetUserId(User)
        );*/
}