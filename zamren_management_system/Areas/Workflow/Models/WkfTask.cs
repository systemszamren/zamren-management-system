using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfTask
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string Reference { get; set; }

    public string ParentTaskId { get; set; } //only updated at initiation
    public WkfTask? ParentTask { get; set; }

    [Required] public string CurrentProcessId { get; set; }
    public WkfProcess CurrentProcess { get; set; }

    [Required] public DateTimeOffset CurrentProcessStartDate { get; set; }
    public DateTimeOffset? CurrentProcessEndDate { get; set; }

    public string? CurrentStepId { get; set; }
    public WkfProcessStep? CurrentStep { get; set; }

    [Required] public DateTimeOffset CurrentStepStartedDate { get; set; }

    public DateTimeOffset? CurrentStepExpectedEndDate { get; set; }

    [Required] public string TaskStartedByUserId { get; set; }
    public ApplicationUser TaskStartedByUser { get; set; }

    public string? CurrentActioningUserId { get; set; }
    public ApplicationUser? CurrentActioningUser { get; set; }
    
    public string? PreviousActioningUserId { get; set; }
    public ApplicationUser? PreviousActioningUser { get; set; }

    [Required] public bool IsOpen { get; set; }
    [Required] public bool WasSentBack { get; set; }
    
    public string? SentBackAtStepId { get; set; }
    public WkfProcessStep? SentBackAtStep { get; set; }

    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    // public IEnumerable<WkfTaskLog>? WkfTaskLogs { get; set; }
    public IEnumerable<WkfTaskComment>? WkfTaskComments { get; set; }
    public IEnumerable<WkfTaskAttachment>? WkfTaskAttachments { get; set; }
    public IEnumerable<WkfTask>? ChildTasks { get; set; }
    public IEnumerable<WkfTaskLog>? TaskWkfTaskLogs { get; set; }
}