namespace zamren_management_system.Areas.Security.Dto;

public class UserRoleDto
{
    public string? UniqueId { get; set; }
    public string? UserId { get; set; }
    public string? RoleId { get; set; }
    public int Counter { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; } 
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? RoleName { get; set; }
    public string? RoleDescription { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string? StartDateString { get; set; }
    public string? EndDateString { get; set; }
    public bool? IsActive { get; set; }
}