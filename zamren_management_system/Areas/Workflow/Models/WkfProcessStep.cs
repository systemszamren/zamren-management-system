using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfProcessStep
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string Name { get; set; }
    [Required] [StringLength(255)] public string Description { get; set; }

    public string? PrivilegeId { get; set; }
    public Privilege? Privilege { get; set; }

    [Required] public string ProcessId { get; set; }
    public WkfProcess Process { get; set; }

    [Required] public bool IsInitialStep { get; set; }

    [Required] public bool IsFinalStep { get; set; }
    [Required] public bool IsDepartmentHeadApproved { get; set; }
    [Required] public bool IsAutoApproved { get; set; }

    // [Required] public bool AssignToSameUserOnSendBack { get; set; } = true;
    
    // [Required] public bool AssignToSameUserOnApprove { get; set; } = true;

    [NotMapped] public bool Ordered { get; set; }
    [NotMapped] public int Order { get; set; }

    public string? PreviousStepId { get; set; }
    public WkfProcessStep? PreviousStep { get; set; }

    public string? NextStepId { get; set; }
    public WkfProcessStep? NextStep { get; set; }

    // public string? NextProcessId { get; set; }
    // public WkfProcess? NextProcess { get; set; }

    // public string? PrevProcessId { get; set; }
    // public WkfProcess? PrevProcess { get; set; }

    [StringLength(255)] public string? RequestMap { get; set; }

    public int? SlaHours { get; set; }

    public string? RoleId { get; set; }
    public ApplicationRole? Role { get; set; }

    public string? ActioningUserId { get; set; }
    public ApplicationUser? ActioningUser { get; set; }

    public string? OfficeId { get; set; }
    public Office? Office { get; set; }

    public string? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }
    public string? ModifiedByUserId { get; set; }
    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public IEnumerable<WkfProcess>? StartingProcesses { get; set; }
    public IEnumerable<WkfProcessStep>? NextSteps { get; set; }
    public IEnumerable<WkfProcessStep>? PreviousSteps { get; set; }
    public IEnumerable<WkfTask>? WkfTasks { get; set; }
    public IEnumerable<WkfTaskLog>? WkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskComment>? WkfTaskComments { get; set; }
    public IEnumerable<WkfTaskAttachment>? WkfTaskAttachments { get; set; }
    public IEnumerable<WkfTask>? WkfTasksSentBackAtStep { get; set; }
    public IEnumerable<WkfTaskLog>? StepWkfTaskLogs { get; set; }
    public IEnumerable<WkfProcessStepCondition>? CurrentStepConditions { get; set; }
    public IEnumerable<WkfProcessStepCondition>? NextStepConditions { get; set; }
}