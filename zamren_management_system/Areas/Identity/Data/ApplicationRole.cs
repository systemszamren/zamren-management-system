using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Security.Models;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Identity.Data;

public sealed class ApplicationRole : IdentityRole
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string name, string description, string createdByUserId, DateTimeOffset createdDate)
        : base(name)
    {
        Description = description;
        CreatedByUserId = createdByUserId;
        CreatedDate = createdDate;
    }

    [StringLength(255)] public string Description { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public IEnumerable<RolePrivilege>? RolePrivileges { get; set; }
    public IEnumerable<ApplicationUserRole>? UserRoles { get; set; }
    public IEnumerable<WkfProcessStep>? WkfSteps { get; set; }
}