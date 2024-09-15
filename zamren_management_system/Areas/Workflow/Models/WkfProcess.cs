using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfProcess
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string Name { get; set; }

    [Required] [StringLength(255)] public string Description { get; set; }

    public string? ParentProcessId { get; set; }
    public WkfProcess? ParentProcess { get; set; }
    [Required] public string ModuleId { get; set; }
    public Module Module { get; set; }

    [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public IEnumerable<WkfProcessStep>? WkfSteps { get; set; }
    public IEnumerable<WkfTask>? WkfCurrentTasks { get; set; }
    public IEnumerable<WkfTask>? WkfPreviousTasks { get; set; }
    public IEnumerable<WkfProcess>? ChildProcesses { get; set; }
}