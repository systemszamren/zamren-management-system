namespace zamren_management_system.Areas.Security.Dto;

public class PasswordHistoryDto
{
    public int Counter { get; set; }
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public DateTimeOffset PasswordCreatedDate { get; set; }
    public DateTimeOffset PasswordExpiryDate { get; set; }
    public string? PasswordExpiryTimeLeft { get; set; }
    public string? UserId { get; set; }
    public string? Status { get; set; }
}