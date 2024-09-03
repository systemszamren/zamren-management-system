using Microsoft.Extensions.Options;
using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.Interfaces;

namespace zamren_management_system.Areas.Common.Services;

public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ISmsService _smsService;
    private readonly SmsOptions _smsOptions;

    public SmsSender(ILogger<SmsSender> logger, IConfiguration configuration, HttpClient httpClient,
        ISmsService smsService, IOptions<SmsOptions> optionsAccessor)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _smsService = smsService;
        _smsOptions = optionsAccessor.Value;
    }

    /*
     EXAMPLE URL TESTED IN POSTMAN
     https://bulksms.zamtel.co.zm/api/v2.1/action/send/api_key/c938d91248fe1ce63e17f4fbe7425d64/contacts/260957548738/senderId/ZAMREN/message/Hello%20world%20from%20bulksms
     */

    public async Task SendSmsAsync(string phoneNumber, string message, string? currentUserId)
    {
        try
        {
            if (_smsService.ShouldThrottle(phoneNumber))
            {
                _logger.LogWarning($"SMS sending to {phoneNumber} throttled due to rate limits.");
                return;
            }

            /*if (true) //DELETE ME!!!!!!!!!!!!!!!!!!!!!!!!!1
            {
                _logger.LogInformation($"Message sent to {phoneNumber}");
                return;
            }*/

            // var baseUrl = _configuration["ZamtelBulkSms:BaseUrl"];
            // var apiKey = _configuration["ZamtelBulkSms:ApiKey"];
            // var senderId = _configuration["ZamtelBulkSms:SenderId"];
            // var url = $"{baseUrl}{apiKey}/contacts/{phoneNumber}/senderId/{senderId}/message/{Uri.EscapeDataString(message)}";

            var baseUrl = _smsOptions.BaseUrl;
            var apiKey = _smsOptions.ApiKey;
            var senderId = _smsOptions.SenderId;
            var url =
                $"{baseUrl}{apiKey}/contacts/{phoneNumber}/senderId/{senderId}/message/{Uri.EscapeDataString(message)}";

            var httpResponse = await _httpClient.GetAsync(url);

            // Construct the SmsResponseDto object
            var responseDto = new SmsResponseDto
            {
                StatusCode = (int)httpResponse.StatusCode,
                Status = httpResponse.IsSuccessStatusCode ? "Success" : "Failed",
                Payload = await httpResponse.Content.ReadAsStringAsync()
            };

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Message sent to {phoneNumber}");
            }
            else
            {
                var errorContent = responseDto.Payload;
                _logger.LogError($"Error sending SMS to {phoneNumber}: {httpResponse.StatusCode} - {errorContent}");
            }

            // Pass the SmsResponseDto object to SaveSms
            await _smsService.SaveSms(senderId, phoneNumber, message, currentUserId, responseDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending SMS");
        }
    }

    //send sms to multiple numbers
    public async Task SendSmsAsync(IEnumerable<string> phoneNumbers, string message, string? currentUserId)
    {
        foreach (var phoneNumber in phoneNumbers)
        {
            await SendSmsAsync(phoneNumber, message, currentUserId);
        }
    }
}