using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfTaskComment
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string TaskId { get; set; }
    public WkfTask Task { get; set; }

    [Required] public string StepId { get; set; }
    public WkfProcessStep Step { get; set; }

    [Required] [DataType(DataType.Text)] public string Comment { get; set; }

    [Required] public string CommentedByUserId { get; set; }
    public ApplicationUser CommentedBy { get; set; }
    
    [Required] public DateTimeOffset CommentedDate { get; set; }
    
    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public IEnumerable<WkfTaskAttachment>? WkfTaskAttachments { get; set; }
}