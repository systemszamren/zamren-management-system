namespace zamren_management_system.Areas.Security.Dto;

public class UserDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public int Counter { get; set; }
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsEmployee { get; set; }
    public string? SupervisorUserId { get; set; }
    public UserDto? Supervisor { get; set; }
    public bool? CanActionWkfTask { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhoneNumberCountryCode { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTimeOffset AccountCreatedDate { get; set; }
    public DateTimeOffset? LastSuccessfulLoginDate { get; set; }
    public DateTimeOffset? LastSuccessfulPasswordChangeDate { get; set; }
    public DateTimeOffset? AccountDeletionScheduledDate { get; set; }
    public bool IsScheduledForDeletion { get; set; }
    public string? Status { get; set; }
    public UserOfficeDto? Office { get; set; }
    public string? ProfilePictureAttachmentPath { get; set; }
}