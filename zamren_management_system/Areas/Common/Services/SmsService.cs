using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Services;

public class SmsService : ISmsService
{
    private readonly AuthContext _context;
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(AuthContext context, ILogger<SmsService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    private async Task<IdentityResult> CreateAsync(SmsNotification smsNotification)
    {
        try
        {
            await _context.SmsNotifications.AddAsync(smsNotification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save sms notification"
            });
        }
    }

    public async Task<IdentityResult> SaveSms(string sender, string recipientPhoneNumber, string message,
        string? currentUserId, SmsResponseDto smsResponseDto)
    {
        try
        {
            if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(recipientPhoneNumber) ||
                string.IsNullOrEmpty(message))
                throw new Exception();

            if (string.IsNullOrEmpty(currentUserId))
                currentUserId = _configuration["SystemAdminAccount:UserId"] ?? string.Empty;

            var currentDateTime = DateTimeOffset.UtcNow;
            var smsNotification = new SmsNotification
            {
                Message = message,
                RecipientPhoneNumber = recipientPhoneNumber,
                Sender = sender,
                IsSent = true,
                DateSent = currentDateTime,
                CreatedByUserId = currentUserId,
                CreatedDate = currentDateTime,
                ResponseStatusCode = smsResponseDto.StatusCode,
                ResponseStatus = smsResponseDto.Status,
                ResponseErrorDescription = smsResponseDto.ErrorDescription,
                ResponsePayload = smsResponseDto.Payload
            };

            return await CreateAsync(smsNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving sms notification");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save sms notification"
            });
        }
    }

    public bool ShouldThrottle(string phoneNumber)
    {
        ConcurrentDictionary<string, (DateTime lastMessageTime, int messageCount)> messageLogs = new();
        var rateLimitPeriodInMinutes =
            TimeSpan.FromMinutes(Convert.ToDouble(_configuration["ZamtelBulkSms:RateLimitPeriodInMinutes"]));
        var maxMessagesPerRateLimitPeriod =
            Convert.ToInt32(_configuration["ZamtelBulkSms:MaxMessagesPerRateLimitPeriod"]);

        var currentTime = DateTime.UtcNow;
        var isThrottled = messageLogs.AddOrUpdate(phoneNumber,
            (currentTime, 1),
            (_, existingValue) =>
            {
                var (lastMessageTime, messageCount) = existingValue;
                return currentTime - lastMessageTime > rateLimitPeriodInMinutes
                    ? (currentTime, 1)
                    : (lastMessageTime, messageCount + 1);
            });

        return isThrottled.messageCount > maxMessagesPerRateLimitPeriod &&
               currentTime - isThrottled.lastMessageTime <= rateLimitPeriodInMinutes;
    }
}