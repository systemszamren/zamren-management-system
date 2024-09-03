using System.ComponentModel.DataAnnotations;

namespace zamren_management_system.Areas.Workflow.Models;

public sealed class WkfSequence
{
    [Key] public long Id { get; set; }

    public long Sequence { get; set; }

    [StringLength(50)] [Required] public string ModuleCode { get; set; }
}