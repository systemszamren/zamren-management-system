using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Procurement.Models;
using zamren_management_system.Areas.Security.Models;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    [PersonalData] [StringLength(255)] public string FirstName { get; set; }

    [PersonalData] [StringLength(255)] public string LastName { get; set; }

    [NotMapped] [PersonalData] public string FullName => $"{FirstName} {LastName}";

    //temp email
    // [PersonalData] [StringLength(255)] public string? TempEmail { get; set; }

    [Required] [PersonalData] public DateTimeOffset AccountCreatedDate { get; set; }
    public DateTimeOffset? AccountDeletionScheduledDate { get; set; }

    [Required] public bool IsScheduledForDeletion { get; set; }

    public DateTimeOffset? LastSuccessfulLoginDate { get; set; }
    public DateTimeOffset? LastSuccessfulPasswordChangeDate { get; set; }

    public bool ChangePasswordOnNextLogin { get; set; }

    public DateTimeOffset? PasswordExpiryDate { get; set; }

    public DateTimeOffset? LastLogoutDate { get; set; }

    [Required] public bool IsEmployee { get; set; }
    [Required] public bool CanActionWkfTasks { get; set; }


    public DateTimeOffset? OutOfOfficeStartDate { get; set; }
    public DateTimeOffset? OutOfOfficeEndDate { get; set; }


    public string? SupervisorUserId { get; set; }
    public ApplicationUser? Supervisor { get; set; }

    [Required] [StringLength(255)] public string RecentActivity { get; set; } = Status.AccountCreated.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public UserDetail? UserDetail { get; set; }
    public IEnumerable<SystemAttachment>? Attachments { get; set; }
    public IEnumerable<SystemAttachment>? CreatedAttachments { get; set; }
    public IEnumerable<SystemAttachment>? ModifiedAttachments { get; set; }
    public IEnumerable<UserDetail>? CreatedUserDetails { get; set; }
    public IEnumerable<UserDetail>? ModifiedUserDetails { get; set; }
    public IEnumerable<Privilege>? CreatedPrivileges { get; set; }
    public IEnumerable<Privilege>? ModifiedPrivileges { get; set; }
    public IEnumerable<RolePrivilege>? CreatedRolePrivileges { get; set; }
    public IEnumerable<RolePrivilege>? ModifiedRolePrivileges { get; set; }
    public IEnumerable<ApplicationRole>? CreatedApplicationRoles { get; set; }
    public IEnumerable<ApplicationRole>? ModifiedApplicationRoles { get; set; }
    public IEnumerable<ApplicationUser>? CreatedApplicationUsers { get; set; }
    public IEnumerable<ApplicationUser>? ModifiedApplicationUsers { get; set; }
    public IEnumerable<Module>? CreatedModules { get; set; }
    public IEnumerable<Module>? ModifiedModules { get; set; }
    public IEnumerable<ModulePrivilege>? CreatedModulePrivileges { get; set; }
    public IEnumerable<ModulePrivilege>? ModifiedModulePrivileges { get; set; }
    public IEnumerable<Office>? CreatedOffices { get; set; }
    public IEnumerable<Office>? ModifiedOffices { get; set; }
    public IEnumerable<Organization>? CreatedOrganizations { get; set; }
    public IEnumerable<Organization>? ModifiedOrganizations { get; set; }
    public IEnumerable<Department>? CreatedDepartments { get; set; }
    public IEnumerable<Department>? ModifiedDepartments { get; set; }
    public IEnumerable<UserOffice>? CreatedUserOffice { get; set; }
    public IEnumerable<UserOffice>? ModifiedUserOffice { get; set; }
    public IEnumerable<UserOffice>? UserOffices { get; set; }
    public IEnumerable<ApplicationUser>? Subordinates { get; set; }
    public IEnumerable<ApplicationUserRole>? CreatedApplicationUserRoles { get; set; }
    public IEnumerable<ApplicationUserRole>? ModifiedApplicationUserRoles { get; set; }
    public IEnumerable<Branch>? ModifiedBranches { get; set; }
    public IEnumerable<Branch>? CreatedBranches { get; set; }
    public IEnumerable<EmailNotification>? CreatedEmailNotifications { get; set; }
    public IEnumerable<EmailNotification>? ModifiedEmailNotifications { get; set; }
    public IEnumerable<EmailNotificationAttachment>? CreatedEmailNotificationAttachments { get; set; }
    public IEnumerable<EmailNotificationAttachment>? ModifiedEmailNotificationAttachments { get; set; }
    public IEnumerable<ApplicationUserRole>? UserRoles { get; set; }
    public IEnumerable<PasswordHistory>? PasswordHistories { get; set; }
    public IEnumerable<PasswordHistory>? CreatedPasswordHistories { get; set; }
    public IEnumerable<PasswordHistory>? ModifiedPasswordHistories { get; set; }
    public IEnumerable<EmailVerification>? EmailVerifications { get; set; }
    public IEnumerable<EmailVerification>? CreatedEmailVerifications { get; set; }
    public IEnumerable<EmailVerification>? ModifiedEmailVerifications { get; set; }
    public IEnumerable<WkfProcess>? CreatedProcesses { get; set; }
    public IEnumerable<WkfProcess>? ModifiedProcesses { get; set; }
    public IEnumerable<WkfProcessStep>? CreatedSteps { get; set; }
    public IEnumerable<WkfProcessStep>? ModifiedSteps { get; set; }
    public IEnumerable<WkfProcessStep>? WkfSteps { get; set; }
    public IEnumerable<WkfTask>? CreatedTasks { get; set; }
    public IEnumerable<WkfTask>? ModifiedTasks { get; set; }
    public IEnumerable<WkfTask>? WkfTasksStartedByUser { get; set; }
    public IEnumerable<WkfTask>? ModifiedWkfTasks { get; set; }
    // public IEnumerable<WkfTaskLog>? WkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskLog>? CreatedWkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskLog>? ModifiedWkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskComment>? WkfTaskComment { get; set; }
    public IEnumerable<WkfTaskComment>? CreatedWkfTaskComment { get; set; }
    public IEnumerable<WkfTaskComment>? ModifiedWkfTaskComment { get; set; }
    public IEnumerable<WkfTaskAttachment>? CreatedWkfTaskAttachments { get; set; }
    public IEnumerable<WkfTaskAttachment>? ModifiedWkfTaskAttachments { get; set; }
    public IEnumerable<WkfTask>? WkfTasksByCurrentUser { get; set; }
    public IEnumerable<SystemNotificationAttachment>? CreatedSystemNotificationAttachments { get; set; }
    public IEnumerable<SystemNotificationAttachment>? ModifiedSystemNotificationAttachments { get; set; }
    public IEnumerable<SystemNotification>? CreatedSystemNotifications { get; set; }
    public IEnumerable<SystemNotification>? ModifiedSystemNotifications { get; set; }
    public IEnumerable<SystemNotification>? SystemNotifications { get; set; }
    public IEnumerable<WkfTaskLog>? ActioningWkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskLog>? NextActioningWkfTaskLogs { get; set; }
    public IEnumerable<EmailNotification>? EmailNotifications { get; set; }
    public IEnumerable<SmsNotification>? CreatedSmsNotifications { get; set; }
    public IEnumerable<SmsNotification>? ModifiedSmsNotifications { get; set; }
    public IEnumerable<SmsNotification>? SmsNotifications { get; set; }
    public IEnumerable<PurchaseRequisitionRequest>? PurchaseRequisitionRequests { get; set; }
    public IEnumerable<PurchaseRequisitionRequest>? CreatedPurchaseRequisitionRequests { get; set; }
    public IEnumerable<PurchaseRequisitionRequest>? ModifiedPurchaseRequisitionRequests { get; set; }
    public IEnumerable<PurchaseRequisitionRequestAttachment>? CreatedPurchaseRequisitionRequestAttachments { get; set; }
    public IEnumerable<PurchaseRequisitionRequestAttachment>? ModifiedPurchaseRequisitionRequestAttachments { get; set; }
    public IEnumerable<WkfProcessStepCondition>? CreatedWkfProcessStepConditions { get; set; }
    public IEnumerable<WkfProcessStepCondition>? ModifiedWkfProcessStepConditions { get; set; }
}