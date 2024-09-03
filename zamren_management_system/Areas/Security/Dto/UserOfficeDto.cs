namespace zamren_management_system.Areas.Security.Dto;

public class UserOfficeDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public int Counter { get; set; }
    public string? UserId { get; set; }
    public string? OfficeId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; } 
    public string? OfficeName { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    
    public string? StartDateString { get; set; }
    public string? EndDateString { get; set; }
}