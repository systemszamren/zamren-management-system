using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using PhoneNumbers;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Common.Services;

public class Util : IUtil
{
    //logger
    private readonly ILogger<Util> _logger;
    private readonly IWebHostEnvironment _env;

    public Util(ILogger<Util> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public DateTimeOffset ConvertStringToDateTimeOffset(string date, bool isEndOfDay = false)
    {
        try
        {
            if (string.IsNullOrEmpty(date))
                return DateTimeOffset.UtcNow;

            var dateTimeOffset = DateTimeOffset.ParseExact(date, date.Contains('-') ? "dd-MM-yyyy" : "dd/MM/yyyy",
                CultureInfo.InvariantCulture);

            // Set the time to the last minute of the day,
            // Set the time to the first minute of the day
            dateTimeOffset = isEndOfDay
                ? dateTimeOffset.Date.AddHours(23).AddMinutes(59)
                : dateTimeOffset.Date.AddHours(0).AddMinutes(0);

            return dateTimeOffset;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Invalid date format");
            return DateTimeOffset.UtcNow;
        }
    }

    public string ConvertDateTimeOffsetToString(DateTimeOffset date)
    {
        return date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
    }

    //trim, remove extra spaces and to upper case
    public string TrimAndRemoveExtraSpacesAndToUpperCase(string value)
    {
        var trimmed = value.Trim();
        var withoutExtraSpaces = Regex.Replace(trimmed, @"\s+", " ");
        return withoutExtraSpaces.ToUpper();
    }

    //get the time left in words from the current time to the expiry time
    public string GetTimeLeftInWords(DateTimeOffset date, string timeUpMessage = "Time up!")
    {
        var timeLeft = date.ToLocalTime() - DateTimeOffset.UtcNow.ToLocalTime();
        string timeLeftInWords;
        if (timeLeft.TotalDays > 365)
        {
            var years = (int)(timeLeft.TotalDays / 365);
            timeLeftInWords = years == 1 ? "1 year left" : $"{years} years left";
        }
        else if (timeLeft.TotalDays > 30)
        {
            var months = (int)(timeLeft.TotalDays / 30);
            timeLeftInWords = months == 1 ? "1 month left" : $"{months} months left";
        }
        else if (timeLeft.Days > 0)
            timeLeftInWords = timeLeft.Days == 1 ? "1 day left" : $"{timeLeft.Days} days left";
        else if (timeLeft.Hours > 0)
            timeLeftInWords = timeLeft.Hours == 1 ? "1 hour left" : $"{timeLeft.Hours} hours left";
        else if (timeLeft.Minutes > 0)
            timeLeftInWords = timeLeft.Minutes == 1 ? "1 minute left" : $"{timeLeft.Minutes} minutes left";
        else if (timeLeft.Seconds > 0)
            timeLeftInWords = timeLeft.Seconds == 1 ? "1 second left" : $"{timeLeft.Seconds} seconds left";
        else
            timeLeftInWords = timeUpMessage;

        return timeLeftInWords;
    }

    //get time ago in words
    public string GetTimeAgoInWords(DateTimeOffset date)
    {
        var timeAgo = DateTimeOffset.UtcNow.ToLocalTime() - date.ToLocalTime();
        string timeAgoInWords;
        if (timeAgo.TotalDays > 365)
        {
            var years = (int)(timeAgo.TotalDays / 365);
            timeAgoInWords = years == 1 ? "1 year ago" : $"{years} years ago";
        }
        else if (timeAgo.TotalDays > 30)
        {
            var months = (int)(timeAgo.TotalDays / 30);
            timeAgoInWords = months == 1 ? "1 month ago" : $"{months} months ago";
        }
        else if (timeAgo.Days > 0)
            timeAgoInWords = timeAgo.Days == 1 ? "1 day ago" : $"{timeAgo.Days} days ago";
        else if (timeAgo.Hours > 0)
            timeAgoInWords = timeAgo.Hours == 1 ? "1 hour ago" : $"{timeAgo.Hours} hours ago";
        else if (timeAgo.Minutes > 0)
            timeAgoInWords = timeAgo.Minutes == 1 ? "1 minute ago" : $"{timeAgo.Minutes} minutes ago";
        else if (timeAgo.Seconds > 0)
            timeAgoInWords = timeAgo.Seconds == 1 ? "1 second ago" : $"{timeAgo.Seconds} seconds ago";
        else
            timeAgoInWords = "Just now";

        return timeAgoInWords;
    }

    public bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public (bool IsValid, string CountryCode, string Response) ValidatePhoneNumber(string phoneNumber,
        string variableName = "Phone number")
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();

        try
        {
            //check if the number does not start with a plus sign
            if (!phoneNumber.StartsWith("+"))
                return (false, string.Empty, variableName + " is not valid. Example format: +260...");

            var number = phoneNumberUtil.Parse(phoneNumber, null);

            if (phoneNumberUtil.IsValidNumber(number) || IsZambianPhoneNumber(phoneNumber))
            {
                var countryCode = phoneNumberUtil.GetRegionCodeForNumber(number);
                return (true, countryCode, variableName + " is valid.");
            }
        }
        catch (NumberParseException)
        {
            return (false, string.Empty, variableName + " is not valid. Example format: +260...");
        }

        return (false, string.Empty, variableName + " is not valid. Example format: +260...");
    }

    //IsZambianPhoneNumber | make sure the phone number starts with +26097 or +26095 or +26096 or +26076 or +26077 and has 12 digits e.g. +26097XXXXXXX
    private static bool IsZambianPhoneNumber(string phoneNumber)
    {
        var zambianPhoneNumberRegex = new Regex(@"^\+260(97|95|96|76|77)\d{7}$");
        return zambianPhoneNumberRegex.IsMatch(phoneNumber);
    }

    public string GetCountryNameByCountryCode(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode)) return string.Empty;
        var regionInfo = new RegionInfo(countryCode);
        return regionInfo.EnglishName;
    }

    public string GetCountryCodeByCountryName(string? countryName)
    {
        if (string.IsNullOrEmpty(countryName)) return string.Empty;
        var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        var culture = Array.Find(cultures, c => new RegionInfo(c.Name).EnglishName == countryName);
        return culture != null ? new RegionInfo(culture.Name).TwoLetterISORegionName : string.Empty;
    }

    public List<CountryDto> GetCountries()
    {
        var countries = new List<CountryDto>();
        var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        var ignoredCountryCodes = new List<string>
            { "BQ", "029", "150", "419", "001", "CW", "XK", "SX", "SS", "BL", "MF" };
        foreach (var culture in cultures)
        {
            var regionInfo = new RegionInfo(culture.Name);
            var country = new CountryDto
            {
                CountryName = regionInfo.EnglishName,
                PhoneCode = GetPhoneCodeByCountryCode(regionInfo.TwoLetterISORegionName),
                CountryCode = regionInfo.TwoLetterISORegionName
            };

            //avoid adding duplicates by checking if the country's TwoLetterISORegionName is already in the list
            if (countries.Any(c => c.CountryCode == country.CountryCode)) continue;

            //ignore country codes in the ignoredCountryCodes list
            if (ignoredCountryCodes.Contains(country.CountryCode)) continue;

            //add country to the list
            countries.Add(country);
        }

        //sort countries by country name
        countries = countries.OrderBy(c => c.CountryName).ToList();

        return countries;
    }

    private string GetPhoneCodeByCountryCode(string countryCode)
    {
        try
        {
            if (string.IsNullOrEmpty(countryCode)) return string.Empty;
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var regionInfo = new RegionInfo(countryCode);
            var phoneCode = phoneNumberUtil.GetCountryCodeForRegion(regionInfo.TwoLetterISORegionName);
            return phoneCode.ToString();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public string GetCountryCodeByPhoneNumber(string? phoneNumber)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();
        try
        {
            var number = phoneNumberUtil.Parse(phoneNumber, null);
            if (phoneNumberUtil.IsValidNumber(number))
            {
                var countryCode = phoneNumberUtil.GetRegionCodeForNumber(number);
                return countryCode;
            }
        }
        catch (NumberParseException)
        {
            return string.Empty;
        }

        return string.Empty;
    }

    public string GenerateUniqueAlphanumericString(int size = 40)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var data = new byte[4 * size];
        using var crypto = RandomNumberGenerator.Create();
        crypto.GetBytes(data);
        var result = new StringBuilder(size);
        for (var i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % chars.Length;
            result.Append(chars[(int)idx]);
        }

        return result.ToString();
    }

    public string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }

    //get baseUrl
    public string GetBaseUrl(HttpRequest request)
    {
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        return baseUrl;
    }

    public string GetWwwRootPath()
    {
        return _env.WebRootPath;
    }

    public string HideEmailAddress(string? email)
    {
        if (string.IsNullOrEmpty(email)) return string.Empty;
        if (!email.Contains('@')) return email;
        if (!IsValidEmail(email)) return email;
        var emailParts = email.Split('@');
        var emailName = emailParts[0];
        var emailDomain = emailParts[1];
        var first2Letters = emailName.Substring(0, 2);
        var lastLetter = emailName.Substring(emailName.Length - 1);
        var hiddenEmailName = $"{first2Letters}***{lastLetter}";
        return $"{hiddenEmailName}@{emailDomain}";
    }
    
    //ConvertDateTimeOffsetToTimeAgo
    public string ConvertDateTimeOffsetToTimeAgo(DateTimeOffset commentCommentedDate)
    {
        return GetTimeAgoInWords(commentCommentedDate);
    }
}