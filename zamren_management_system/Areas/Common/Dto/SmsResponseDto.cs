namespace zamren_management_system.Areas.Common.Dto;

public class SmsResponseDto
{
    public int? StatusCode { get; set; }
    public string? Status { get; set; }
    public string? ErrorDescription { get; set; }
    public string? Payload { get; set; }
}