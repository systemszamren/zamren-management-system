using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Security.Models;

public sealed class Privilege
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [StringLength(255)] public string Name { get; set; }

    [Required] [StringLength(255)] public string Description { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public IEnumerable<RolePrivilege>? RolePrivileges { get; set; }
    // public IEnumerable<ModulePrivilege>? ModulePrivileges { get; set; }
    
    public ICollection<ModulePrivilege> ModulePrivileges { get; set; } = new List<ModulePrivilege>();

    public IEnumerable<WkfProcessStep>? WkfProcessSteps { get; set; }
}