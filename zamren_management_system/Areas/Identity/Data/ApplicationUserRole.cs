using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace zamren_management_system.Areas.Identity.Data;

public sealed class ApplicationUserRole : IdentityUserRole<string>
{
    [Required] public string UniqueId { get; set; } = Guid.NewGuid().ToString();
    
    public ApplicationUser User { get; set; }
    public ApplicationRole Role { get; set; }
    [Required] public DateTimeOffset StartDate { get; set; }

    [Required] public DateTimeOffset EndDate { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}