using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Procurement.Models;
using zamren_management_system.Areas.Security.Models;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Identity.Data;

public class AuthContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
    ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public AuthContext(DbContextOptions<AuthContext> options)
        : base(options)
    {
    }

    public DbSet<SystemAttachment> SystemAttachments { get; set; }
    public DbSet<UserDetail> UserDetails { get; set; }
    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<RolePrivilege> RolePrivileges { get; set; }
    public DbSet<ModulePrivilege> ModulePrivileges { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Office> Offices { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<UserOffice> UserOffices { get; set; }
    public DbSet<EmailNotification> EmailNotifications { get; set; }
    public DbSet<EmailNotificationAttachment> EmailNotificationAttachments { get; set; }
    public DbSet<SystemNotification> SystemNotifications { get; set; }
    public DbSet<SmsNotification> SmsNotifications { get; set; }
    public DbSet<SystemNotificationAttachment> SystemNotificationAttachments { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<WkfProcess> WkfProcesses { get; set; }
    public DbSet<WkfTaskComment> WkfTaskComments { get; set; }
    public DbSet<WkfTaskAttachment> WkfTaskAttachments { get; set; }
    public DbSet<WkfProcessStep> WkfProcessSteps { get; set; }

    public DbSet<WkfProcessStepCondition> WkfProcessStepConditions { get; set; }
    public DbSet<WkfTask> WkfTasks { get; set; }
    public DbSet<WkfTaskLog> WkfTaskLogs { get; set; }
    public DbSet<WkfSequence> WkfSequences { get; set; }

    public DbSet<PurchaseRequisitionRequest> PurchaseRequisitionRequests { get; set; }
    public DbSet<PurchaseRequisitionRequestAttachment> PurchaseRequisitionRequestAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //change the default table names
        builder.Entity<IdentityRoleClaim<string>>().ToTable("sec_role_claim");
        builder.Entity<IdentityUserClaim<string>>().ToTable("sec_user_claim");
        builder.Entity<IdentityUserLogin<string>>().ToTable("sec_user_login");
        builder.Entity<IdentityUserToken<string>>().ToTable("sec_user_token");


        //PasswordHistory   
        builder.Entity<PasswordHistory>().ToTable("sec_password_history");
        builder.Entity<PasswordHistory>()
            .HasOne(x => x.User)
            .WithMany(x => x.PasswordHistories)
            .HasForeignKey(x => x.UserId);

        builder.Entity<PasswordHistory>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedPasswordHistories)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<PasswordHistory>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedPasswordHistories)
            .HasForeignKey(x => x.ModifiedByUserId);

        //EmailVerification
        builder.Entity<EmailVerification>().ToTable("gen_email_verification");
        builder.Entity<EmailVerification>()
            .HasOne(x => x.User)
            .WithMany(x => x.EmailVerifications)
            .HasForeignKey(x => x.UserId);

        builder.Entity<EmailVerification>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedEmailVerifications)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<EmailVerification>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedEmailVerifications)
            .HasForeignKey(x => x.ModifiedByUserId);

        //Attachment
        builder.Entity<SystemAttachment>().ToTable("gen_system_attachment");
        builder.Entity<SystemAttachment>()
            .HasOne(x => x.UploadedBy)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.UploadedByUserId);

        builder.Entity<SystemAttachment>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedAttachments)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<SystemAttachment>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedAttachments)
            .HasForeignKey(x => x.ModifiedByUserId);

        //SmsNotification
        builder.Entity<SmsNotification>().ToTable("gen_sms_notification");
        builder.Entity<SmsNotification>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedSmsNotifications)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<SmsNotification>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedSmsNotifications)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<SmsNotification>()
            .HasOne(x => x.RecipientUser)
            .WithMany(x => x.SmsNotifications)
            .HasForeignKey(x => x.RecipientUserId);

        //EmailNotification
        builder.Entity<EmailNotification>().ToTable("gen_email_notification");
        builder.Entity<EmailNotification>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedEmailNotifications)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<EmailNotification>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedEmailNotifications)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<EmailNotification>()
            .HasOne(x => x.RecipientUser)
            .WithMany(x => x.EmailNotifications)
            .HasForeignKey(x => x.RecipientUserId);

        //EmailNotificationAttachment
        builder.Entity<EmailNotificationAttachment>().ToTable("gen_email_notification_attachment");
        builder.Entity<EmailNotificationAttachment>()
            .HasOne(x => x.SystemAttachment)
            .WithMany(x => x.EmailNotificationAttachments)
            .HasForeignKey(x => x.AttachmentId);

        builder.Entity<EmailNotificationAttachment>()
            .HasOne(x => x.EmailNotification)
            .WithMany(x => x.EmailNotificationAttachments)
            .HasForeignKey(x => x.EmailNotificationId);

        builder.Entity<EmailNotificationAttachment>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedEmailNotificationAttachments)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<EmailNotificationAttachment>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedEmailNotificationAttachments)
            .HasForeignKey(x => x.ModifiedByUserId);

        //SystemNotification
        builder.Entity<SystemNotification>().ToTable("gen_system_notification");
        builder.Entity<SystemNotification>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedSystemNotifications)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<SystemNotification>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedSystemNotifications)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<SystemNotification>()
            .HasOne(x => x.Recipient)
            .WithMany(x => x.SystemNotifications)
            .HasForeignKey(x => x.RecipientUserId);

        //SystemNotificationAttachment
        builder.Entity<SystemNotificationAttachment>().ToTable("gen_system_notification_attachment");
        builder.Entity<SystemNotificationAttachment>()
            .HasOne(x => x.SystemAttachment)
            .WithMany(x => x.SystemNotificationAttachments)
            .HasForeignKey(x => x.AttachmentId);

        builder.Entity<SystemNotificationAttachment>()
            .HasOne(x => x.SystemNotification)
            .WithMany(x => x.SystemNotificationAttachments)
            .HasForeignKey(x => x.SystemNotificationId);

        builder.Entity<SystemNotificationAttachment>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedSystemNotificationAttachments)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<SystemNotificationAttachment>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedSystemNotificationAttachments)
            .HasForeignKey(x => x.ModifiedByUserId);

        //UserDetail
        builder.Entity<UserDetail>().ToTable("sec_user_detail");
        builder.Entity<UserDetail>()
            .HasOne(x => x.User)
            .WithOne(x => x.UserDetail)
            .HasForeignKey<UserDetail>(x => x.UserId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.ProfilePictureAttachment)
            .WithOne(x => x.ProfilePictureAttachmentUser)
            .HasForeignKey<UserDetail>(x => x.ProfilePictureAttachmentId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.IdentityAttachment)
            .WithOne(x => x.IdentityAttachmentUser)
            .HasForeignKey<UserDetail>(x => x.IdentityAttachmentId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.ProofOfResidencyAttachment)
            .WithOne(x => x.ProofOfResidencyAttachmentUser)
            .HasForeignKey<UserDetail>(x => x.ProofOfResidencyAttachmentId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.NextOfKinIdentityAttachment)
            .WithOne(x => x.NextOfKinIdentityAttachmentUser)
            .HasForeignKey<UserDetail>(x => x.NextOfKinIdentityAttachmentId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.NextOfKinProofOfResidencyAttachment)
            .WithOne(x => x.NextOfKinProofOfResidencyAttachmentUser)
            .HasForeignKey<UserDetail>(x => x.NextOfKinProofOfResidencyAttachmentId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedUserDetails)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<UserDetail>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedUserDetails)
            .HasForeignKey(x => x.ModifiedByUserId);

        //Privilege
        builder.Entity<Privilege>().ToTable("sec_privilege");
        builder.Entity<Privilege>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedPrivileges)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<Privilege>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedPrivileges)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<Privilege>()
            .HasIndex(x => x.Name)
            .IsUnique();

        //Module
        builder.Entity<Module>().ToTable("sec_module");
        builder.Entity<Module>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedModules)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<Module>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedModules)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<Module>()
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.Entity<Module>()
            .HasIndex(x => x.Code)
            .IsUnique();

        //RolePrivilege
        builder.Entity<RolePrivilege>().ToTable("sec_role_privilege");
        builder.Entity<RolePrivilege>()
            .HasKey(x => new { x.RoleId, x.PrivilegeId });

        builder.Entity<RolePrivilege>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePrivileges)
            .HasForeignKey(rp => rp.RoleId);

        builder.Entity<RolePrivilege>()
            .HasOne(rp => rp.Privilege)
            .WithMany(p => p.RolePrivileges)
            .HasForeignKey(rp => rp.PrivilegeId);

        builder.Entity<RolePrivilege>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedRolePrivileges)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<RolePrivilege>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedRolePrivileges)
            .HasForeignKey(x => x.ModifiedByUserId);

        //ModulePrivilege
        builder.Entity<ModulePrivilege>().ToTable("sec_module_privilege");
        builder.Entity<ModulePrivilege>()
            .HasKey(x => new { x.ModuleId, x.PrivilegeId });

        builder.Entity<ModulePrivilege>()
            .HasOne(rp => rp.Module)
            .WithMany(r => r.ModulePrivileges)
            .HasForeignKey(rp => rp.ModuleId);

        builder.Entity<ModulePrivilege>()
            .HasOne(rp => rp.Privilege)
            .WithMany(p => p.ModulePrivileges)
            .HasForeignKey(rp => rp.PrivilegeId);

        builder.Entity<ModulePrivilege>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedModulePrivileges)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<ModulePrivilege>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedModulePrivileges)
            .HasForeignKey(x => x.ModifiedByUserId);

        //ApplicationRole
        builder.Entity<ApplicationRole>().ToTable("sec_role");
        builder.Entity<ApplicationRole>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedApplicationRoles)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<ApplicationRole>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedApplicationRoles)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<ApplicationRole>()
            .HasIndex(x => x.Name)
            .IsUnique();

        //ApplicationUserRole
        builder.Entity<ApplicationUserRole>().ToTable("sec_user_role");
        builder.Entity<ApplicationUserRole>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedApplicationUserRoles)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<ApplicationUserRole>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedApplicationUserRoles)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<ApplicationUserRole>()
            .HasIndex(x => x.UniqueId)
            .IsUnique();

        builder.Entity<ApplicationUserRole>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId);

        builder.Entity<ApplicationUserRole>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);

        //ApplicationUser
        builder.Entity<ApplicationUser>().ToTable("sec_user");
        builder.Entity<ApplicationUser>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedApplicationUsers)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<ApplicationUser>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedApplicationUsers)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<ApplicationUser>()
            .HasOne(x => x.Supervisor)
            .WithMany(x => x.Subordinates)
            .HasForeignKey(x => x.SupervisorUserId);

        //Organization
        builder.Entity<Organization>().ToTable("sec_organization");
        builder.Entity<Organization>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedOrganizations)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<Organization>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedOrganizations)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<Organization>()
            .HasIndex(x => x.Name)
            .IsUnique();

        //Branch
        builder.Entity<Branch>().ToTable("sec_branch");
        builder.Entity<Branch>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedBranches)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<Branch>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedBranches)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<Branch>()
            .HasOne(x => x.Organization)
            .WithMany(x => x.Branches
            ).HasForeignKey(x => x.OrganizationId);

        builder.Entity<Branch>()
            .HasIndex(x => x.Name)
            .IsUnique();

        //Department
        builder.Entity<Department>().ToTable("sec_department");
        builder.Entity<Department>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedDepartments)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<Department>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedDepartments)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<Department>()
            .HasOne(x => x.Branch)
            .WithMany(x => x.Departments)
            .HasForeignKey(x => x.BranchId);

        builder.Entity<Department>()
            .HasIndex(x => x.Name)
            .IsUnique();

        //Office
        builder.Entity<Office>().ToTable("sec_office");
        builder.Entity<Office>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedOffices)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<Office>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedOffices)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<Office>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Offices)
            .HasForeignKey(x => x.DepartmentId);

        builder.Entity<Office>()
            .HasIndex(x => x.Name)
            .IsUnique();

        //UserOffice
        builder.Entity<UserOffice>().ToTable("sec_user_office");
        builder.Entity<UserOffice>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedUserOffice)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<UserOffice>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedUserOffice)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<UserOffice>()
            .HasOne(uo => uo.User)
            .WithMany(u => u.UserOffices)
            .HasForeignKey(uo => uo.UserId);

        builder.Entity<UserOffice>()
            .HasOne(uo => uo.Office)
            .WithMany(o => o.UserOffices)
            .HasForeignKey(uo => uo.OfficeId);

        //WkfSequence
        builder.Entity<WkfSequence>().ToTable("wkf_sequence");

        //WkfProcess
        builder.Entity<WkfProcess>().ToTable("wkf_process");
        builder.Entity<WkfProcess>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedProcesses)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfProcess>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedProcesses)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<WkfProcess>()
            .HasOne(x => x.Module)
            .WithMany(x => x.WkfProcesses)
            .HasForeignKey(x => x.ModuleId);

        builder.Entity<WkfProcess>()
            .HasOne(x => x.ParentProcess)
            .WithMany(x => x.ChildProcesses)
            .HasForeignKey(x => x.ParentProcessId);

        //WkfProcessStep
        builder.Entity<WkfProcessStep>().ToTable("wkf_process_step");
        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedSteps)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedSteps)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Process)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.ProcessId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Role)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.RoleId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.ActioningUser)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.ActioningUserId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Office)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.OfficeId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Department)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.DepartmentId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Branch)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.BranchId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Organization)
            .WithMany(x => x.WkfSteps)
            .HasForeignKey(x => x.OrganizationId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.PreviousStep)
            .WithMany(x => x.NextSteps)
            .HasForeignKey(x => x.PreviousStepId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.NextStep)
            .WithMany(x => x.PreviousSteps)
            .HasForeignKey(x => x.NextStepId);

        /*builder.Entity<WkfProcessStep>()
            .HasOne(x => x.NextProcess)
            .WithMany(x => x.PreviousSteps)
            .HasForeignKey(x => x.NextProcessId);

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.PrevProcess)
            .WithMany(x => x.NextSteps)
            .HasForeignKey(x => x.PrevProcessId);*/

        builder.Entity<WkfProcessStep>()
            .HasOne(x => x.Privilege)
            .WithMany(x => x.WkfProcessSteps)
            .HasForeignKey(x => x.PrivilegeId);

        //WkfProcessStepCondition
        builder.Entity<WkfProcessStepCondition>().ToTable("wkf_process_step_condition");
        builder.Entity<WkfProcessStepCondition>()
            .HasOne(x => x.CurrentStep)
            .WithMany(x => x.CurrentStepConditions)
            .HasForeignKey(x => x.CurrentStepId);

        builder.Entity<WkfProcessStepCondition>()
            .HasOne(x => x.NextStep)
            .WithMany(x => x.NextStepConditions)
            .HasForeignKey(x => x.NextStepId);

        builder.Entity<WkfProcessStepCondition>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedWkfProcessStepConditions)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfProcessStepCondition>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedWkfProcessStepConditions)
            .HasForeignKey(x => x.ModifiedByUserId);

        //WkfTask
        builder.Entity<WkfTask>().ToTable("wkf_task");
        builder.Entity<WkfTask>()
            .HasOne(x => x.ParentTask)
            .WithMany(x => x.ChildTasks)
            .HasForeignKey(x => x.ParentTaskId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedTasks)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.CurrentProcess)
            .WithMany(x => x.WkfCurrentTasks)
            .HasForeignKey(x => x.CurrentProcessId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.CurrentStep)
            .WithMany(x => x.WkfTasks)
            .HasForeignKey(x => x.CurrentStepId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.SentBackAtStep)
            .WithMany(x => x.WkfTasksSentBackAtStep)
            .HasForeignKey(x => x.SentBackAtStepId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.TaskStartedByUser)
            .WithMany(x => x.WkfTasksStartedByUser)
            .HasForeignKey(x => x.TaskStartedByUserId);

        builder.Entity<WkfTask>()
            .HasOne(x => x.CurrentActioningUser)
            .WithMany(x => x.WkfTasksByCurrentUser)
            .HasForeignKey(x => x.CurrentActioningUserId);

        builder.Entity<WkfTask>()
            .HasIndex(x => x.Reference)
            .IsUnique();

        //WkfTaskLog
        builder.Entity<WkfTaskLog>().ToTable("wkf_task_log");
        builder.Entity<WkfTaskLog>()
            .HasOne(x => x.Task)
            .WithMany(x => x.TaskWkfTaskLogs)
            .HasForeignKey(x => x.TaskId);

        builder.Entity<WkfTaskLog>()
            .HasOne(x => x.Step)
            .WithMany(x => x.StepWkfTaskLogs)
            .HasForeignKey(x => x.StepId);

        builder.Entity<WkfTaskLog>()
            .HasOne(x => x.ActioningUser)
            .WithMany(x => x.ActioningWkfTaskLogs)
            .HasForeignKey(x => x.ActioningUserId);

        builder.Entity<WkfTaskLog>()
            .HasOne(x => x.NextActioningUser)
            .WithMany(x => x.NextActioningWkfTaskLogs)
            .HasForeignKey(x => x.NextActioningUserId);

        builder.Entity<WkfTaskLog>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedWkfTaskLogs)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfTaskLog>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedWkfTaskLogs)
            .HasForeignKey(x => x.ModifiedByUserId);

        //WkfTaskComment
        builder.Entity<WkfTaskComment>().ToTable("wkf_task_comment");
        builder.Entity<WkfTaskComment>()
            .HasOne(x => x.Task)
            .WithMany(x => x.WkfTaskComments)
            .HasForeignKey(x => x.TaskId);

        builder.Entity<WkfTaskComment>()
            .HasOne(x => x.Step)
            .WithMany(x => x.WkfTaskComments)
            .HasForeignKey(x => x.StepId);

        builder.Entity<WkfTaskComment>()
            .HasOne(x => x.CommentedBy)
            .WithMany(x => x.WkfTaskComment)
            .HasForeignKey(x => x.CommentedByUserId);

        builder.Entity<WkfTaskComment>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedWkfTaskComment)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfTaskComment>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedWkfTaskComment)
            .HasForeignKey(x => x.ModifiedByUserId);

        //WkfTaskAttachment
        builder.Entity<WkfTaskAttachment>().ToTable("wkf_task_attachment");
        builder.Entity<WkfTaskAttachment>()
            .HasOne(x => x.Task)
            .WithMany(x => x.WkfTaskAttachments)
            .HasForeignKey(x => x.TaskId);

        builder.Entity<WkfTaskAttachment>()
            .HasOne(x => x.Step)
            .WithMany(x => x.WkfTaskAttachments)
            .HasForeignKey(x => x.StepId);

        builder.Entity<WkfTaskAttachment>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedWkfTaskAttachments)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<WkfTaskAttachment>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedWkfTaskAttachments)
            .HasForeignKey(x => x.ModifiedByUserId);

        //PurchaseRequisitionRequest
        builder.Entity<PurchaseRequisitionRequest>().ToTable("proc_purchase_requisition_request");
        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.RequestingOfficerUser)
            .WithMany(x => x.PurchaseRequisitionRequests)
            .HasForeignKey(x => x.RequestingOfficerUserId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.Organization)
            .WithMany(x => x.PurchaseRequisitionRequests)
            .HasForeignKey(x => x.OrganizationId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.Branch)
            .WithMany(x => x.PurchaseRequisitionRequests)
            .HasForeignKey(x => x.BranchId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.Department)
            .WithMany(x => x.PurchaseRequisitionRequests)
            .HasForeignKey(x => x.DepartmentId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.Office)
            .WithMany(x => x.PurchaseRequisitionRequests)
            .HasForeignKey(x => x.OfficeId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedPurchaseRequisitionRequests)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedPurchaseRequisitionRequests)
            .HasForeignKey(x => x.ModifiedByUserId);

        builder.Entity<PurchaseRequisitionRequest>()
            .HasIndex(x => x.Reference)
            .IsUnique();

        //PurchaseRequisitionRequestAttachment
        builder.Entity<PurchaseRequisitionRequestAttachment>().ToTable("proc_purchase_requisition_request_attachment");

        builder.Entity<PurchaseRequisitionRequestAttachment>()
            .HasOne(x => x.PurchaseRequisitionRequest)
            .WithMany(x => x.PurchaseRequisitionRequestAttachments)
            .HasForeignKey(x => x.PurchaseRequisitionRequestId);

        builder.Entity<PurchaseRequisitionRequestAttachment>()
            .HasOne(x => x.SystemAttachment)
            .WithMany(x => x.PurchaseRequisitionRequestAttachments)
            .HasForeignKey(x => x.AttachmentId);

        builder.Entity<PurchaseRequisitionRequestAttachment>()
            .HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedPurchaseRequisitionRequestAttachments)
            .HasForeignKey(x => x.CreatedByUserId);

        builder.Entity<PurchaseRequisitionRequestAttachment>()
            .HasOne(x => x.ModifiedBy)
            .WithMany(x => x.ModifiedPurchaseRequisitionRequestAttachments)
            .HasForeignKey(x => x.ModifiedByUserId);
    }
}