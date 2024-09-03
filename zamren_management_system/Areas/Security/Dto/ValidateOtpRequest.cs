namespace zamren_management_system.Areas.Security.Dto;

public class ValidateOtpRequest
{
    public string PhoneNumber { get; set; }
    public string Otp { get; set; }
}