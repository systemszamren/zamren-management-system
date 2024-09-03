using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfTaskLog
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string TaskId { get; set; }
    public WkfTask Task { get; set; }

    [Required] public string StepId { get; set; }
    public WkfProcessStep Step { get; set; }

    // public DateTimeOffset? StepStartedDate { get; set; }
    // public DateTimeOffset? StepEndedDate { get; set; }

    public string? ActioningUserId { get; set; }
    public ApplicationUser? ActioningUser { get; set; }

    public string? NextActioningUserId { get; set; }
    public ApplicationUser? NextActioningUser { get; set; }

    [Required] [StringLength(255)] public string Action { get; set; }

    [Required] public DateTimeOffset ActionDate { get; set; }

    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}