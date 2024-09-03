namespace zamren_management_system.Areas.Security.Interfaces;

public interface IOtpService
{
    /// <summary>
    ///     Generates an OTP and stores it in the cache.
    /// </summary>
    /// <param name="key"> The unique key to store the OTP in the cache.</param>
    /// <returns> The generated OTP.</returns>
    string GenerateOtp(string key);

    /// <summary>
    ///     Validates an OTP.
    /// </summary>
    /// <param name="key"> The key to retrieve the OTP from the cache.</param>
    /// <param name="otp"> The OTP to validate.</param>
    /// <returns> True if the OTP is valid, otherwise false.</returns>
    bool IsValidOtp(string key, string otp);
}