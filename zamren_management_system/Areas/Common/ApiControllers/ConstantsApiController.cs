using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Common.ApiControllers;

[ApiController]
[Area("Common")]
[Route("api/common/constants")]
public class ConstantsApiController : ControllerBase
{
    private readonly ILogger<ConstantsApiController> _logger;
    private readonly IUtil _util;

    public ConstantsApiController(ILogger<ConstantsApiController> logger, IUtil util)
    {
        _logger = logger;
        _util = util;
    }

    [HttpGet("get-genders")]
    public IActionResult GetGenders()
    {
        try
        {
            var genders = Enum.GetValues<Gender>().Select(gender => gender.ToString()).ToList();

            return Ok(new { success = true, genders });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpGet("get-identity-types")]
    public IActionResult GetIdentityTypes()
    {
        try
        {
            var identityTypes = Enum.GetValues<IdentityType>().Select(identityType => new IdentityTypeDto
            {
                Value = identityType.ToString(),
                Text = identityType.GetDescription()
            }).ToList();

            return Ok(new { success = true, identityTypes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpGet("get-countries")]
    public IActionResult GetCountries()
    {
        try
        {
            var countries = _util.GetCountries();
            return Ok(new { success = true, countries });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get country by phone code
    [HttpGet("get-country-by-phone-code")]
    public IActionResult GetCountryByPhoneCode([FromQuery] string phoneCode)
    {
        try
        {
            var country = _util.GetCountries().FirstOrDefault(c => c.PhoneCode == phoneCode);
            return Ok(new { success = true, country });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}