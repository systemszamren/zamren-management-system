using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Security.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    
    public OtpService(IMemoryCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    public string GenerateOtp(string key)
    {
        var otpLength = Convert.ToInt32(_configuration["Otp:Length"]);
        var expireInMinutes = Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]);
        var maxOtpRequestsPerHour = Convert.ToInt32(_configuration["Otp:MaxRequestsPerHour"]);
        var rateLimitWindowMinutes = Convert.ToInt32(_configuration["Otp:RateLimitWindowInMinutes"]);

        var cacheEntry = _cache.Get<OtpRequestCacheEntry>(key) ?? new OtpRequestCacheEntry
        {
            RequestCount = 0,
            LastRequestTime = DateTimeOffset.UtcNow
        };

        // Check rate limit
        if (cacheEntry.RequestCount >= maxOtpRequestsPerHour &&
            DateTimeOffset.UtcNow - cacheEntry.LastRequestTime < TimeSpan.FromMinutes(rateLimitWindowMinutes))
        {
            throw new InvalidOperationException("Rate limit exceeded. Please try again later.");
        }

        if (DateTimeOffset.UtcNow - cacheEntry.LastRequestTime >= TimeSpan.FromMinutes(rateLimitWindowMinutes))
        {
            cacheEntry.RequestCount = 0; // Reset count after window expires
        }

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var otp = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, otpLength);
        var otpString = otp.ToString($"D{otpLength}");

        _cache.Set(key, otpString, TimeSpan.FromMinutes(expireInMinutes));

        cacheEntry.RequestCount++;
        cacheEntry.LastRequestTime = DateTimeOffset.UtcNow;
        _cache.Set(key + "_rate", cacheEntry, TimeSpan.FromMinutes(rateLimitWindowMinutes));

        return otpString;
    }

    public bool IsValidOtp(string key, string otp)
    {
        if (!_cache.TryGetValue(key, out string? cachedOtp)) return false;
        if (cachedOtp != otp) return false;
        _cache.Remove(key); // Remove OTP after successful validation
        return true;
    }
}