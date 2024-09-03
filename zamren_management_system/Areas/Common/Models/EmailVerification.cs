using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Models;

public sealed class EmailVerification
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required] [StringLength(255)] public string Email { get; set; }
    [Required] [DataType(DataType.Text)] public string Token { get; set; }
    [Required] public string EmailType { get; set; }
    [Required] public bool IsVerified { get; set; }
    [Required] public DateTimeOffset? DateInitiated { get; set; }
    public DateTimeOffset? DateVerified { get; set; }
    [Required] public DateTimeOffset ExpiryDate { get; set; }
    [Required] [StringLength(255)] public string UserId { get; set; }
    [Required] public ApplicationUser User { get; set; }
    
    [Required] [StringLength(255)] public string Status { get; set; } = Enums.Status.Active.ToString();
    [Required] public string CreatedByUserId { get; set; }
    public ApplicationUser CreatedBy { get; set; }
    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}