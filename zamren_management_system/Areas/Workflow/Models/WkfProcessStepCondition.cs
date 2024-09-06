using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfProcessStepCondition
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string CurrentStepId { get; set; }
    public WkfProcessStep CurrentStep { get; set; }

    public string NextStepId { get; set; }
    public WkfProcessStep NextStep { get; set; }

    [Required] [StringLength(255)] public string Condition { get; set; }

    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }
    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }
    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}