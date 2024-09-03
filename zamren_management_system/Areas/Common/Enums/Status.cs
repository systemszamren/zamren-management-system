namespace zamren_management_system.Areas.Common.Enums;

public enum Status
{
    Active,
    ReversedDeletion,
    ScheduledForDeletion,
    Locked,
    AccountUnlocked,
    LoggedOut,
    EmailConfirmed,
    AccountEdited,
    LoggedInUsingPassword,
    LoggedInUsingGoogle,
    AccountCreatedUsingGoogle,
    PasswordChanged,
    AccountCreated,
    PasswordExpired,
    Expired
}