using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Security.Models;

[PrimaryKey(nameof(ModuleId), nameof(PrivilegeId))]
public sealed class ModulePrivilege
{
    [Column(Order = 0)] public string ModuleId { get; set; }

    [Column(Order = 1)] public string PrivilegeId { get; set; }

    [ForeignKey("ModuleId")] public Module Module { get; set; }

    [ForeignKey("PrivilegeId")] public Privilege Privilege { get; set; }

    [Required]
    [StringLength(255)]
    public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required]
    public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required]
    public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}