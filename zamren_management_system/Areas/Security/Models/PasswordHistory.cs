using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Security.Models;

public sealed class PasswordHistory
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string PasswordHash { get; set; }

    [Required] public DateTimeOffset PasswordCreatedDate { get; set; }

    [Required] public DateTimeOffset PasswordExpiryDate { get; set; }

    [Required] public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}