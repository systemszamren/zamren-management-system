using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface IUtil
{
    /// <summary>
    ///     Converts a string to a DateTimeOffset object. Set the time to the last minute of the day if isEndOfDay is true.
    /// </summary>
    /// <param name="date"> The string to convert.</param>
    /// <param name="isEndOfDay"></param>
    /// <returns> The DateTimeOffset object.</returns>
    public DateTimeOffset ConvertStringToDateTimeOffset(string date, bool isEndOfDay = false);

    /// <summary>
    ///     Converts a DateTimeOffset object to a string.
    /// </summary>
    /// <param name="date"> The DateTimeOffset object.</param>
    /// <returns> The string.</returns>
    public string ConvertDateTimeOffsetToString(DateTimeOffset date);

    /// <summary>
    ///   Trims and removes extra spaces from a string.
    /// </summary>
    /// <param name="value"> The string to trim.</param>
    /// <returns> The trimmed string.</returns>
    public string TrimAndRemoveExtraSpacesAndToUpperCase(string value);

    /// <summary>
    ///     Gets the time left in words from the current datetime to the specific datetime.
    /// </summary>
    /// <param name="date"> The specific date.</param>
    /// <param name="timeUpMessage"></param>
    /// <returns> The time left in words.</returns>
    public string GetTimeLeftInWords(DateTimeOffset date, string timeUpMessage);

    /// <summary>
    ///     Gets the time ago in words from the current datetime to the specific datetime.
    /// </summary>
    /// <param name="date"> The specific date.</param>
    /// <returns> The time ago in words.</returns>
    public string GetTimeAgoInWords(DateTimeOffset date);

    /// <summary>
    ///     Validates an email address.
    /// </summary>
    /// <param name="email"> The email address to validate.</param>
    /// <returns> True if the email is valid, otherwise false.</returns>
    bool IsValidEmail(string email);

    /// <summary>
    ///  Validates a phone number.
    /// </summary>
    /// <param name="phoneNumber"> The phone number to validate.</param>
    /// <param name="variableName"> The name of the variable containing the phone number.</param>
    /// <returns> A tuple containing a boolean indicating if the phone number is valid, the country code and the response message.</returns>
    public (bool IsValid, string CountryCode, string Response) ValidatePhoneNumber(string phoneNumber,
        string variableName = "Phone number");

    /// <summary>
    ///     Gets the country name by the country code.
    /// </summary>
    /// <param name="countryCode"></param>
    /// <returns> The country name.</returns>
    public string GetCountryNameByCountryCode(string? countryCode);

    /// <summary>
    ///     Gets the country code by the country name.
    /// </summary>
    /// <param name="countryName"></param>
    /// <returns> The country code.</returns>
    public string GetCountryCodeByCountryName(string? countryName);

    /// <summary>
    ///     Get country name, phone code and country code.
    /// </summary>
    /// <returns> A list of country details.</returns>
    public List<CountryDto> GetCountries();

    /// <summary>
    ///     Get country code by phone number.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <returns> The country code.</returns>
    public string GetCountryCodeByPhoneNumber(string? phoneNumber);

    /// <summary>
    ///     Generates a unique alphanumeric string.
    /// </summary>
    /// <param name="size"> The size of the string. Default size is [50]. E.g: 'Qw35f9aK3WY1oP2s3d4f5g6h7d8k9l0Z1x2c3v4b5N6m7uO9E5c'</param>
    /// <returns> The unique token.</returns>
    public string GenerateUniqueAlphanumericString(int size = 50);

    /// <summary>
    ///     Generates a Guid string. e.g 'bu723b76-e931-4c81-9f1b-652315d77202'
    /// </summary>
    /// <returns> The Guid string.</returns>
    public string GenerateGuid();

    /// <summary>
    ///     Hides the email address by replacing the middle characters with 3 asterisks. e.g., john.banda@gmail.com -> jo***a@gmail.com'
    /// </summary>
    /// <param name="email"></param>
    /// <returns> The hidden email address</returns>
    string HideEmailAddress(string? email);

    /// <summary>
    ///     Gets the base URL of the application.
    /// </summary>
    /// <param name="request"> The HttpRequest object.</param>
    /// <returns> The base URL.</returns>
    string GetBaseUrl(HttpRequest request);

    /// <summary>
    ///     Gets absolute path of wwwroot folder.
    /// </summary>
    /// <returns> The absolute path of wwwroot folder.</returns>
    public string GetWwwRootPath();

}