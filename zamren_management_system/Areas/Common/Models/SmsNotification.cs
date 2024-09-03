using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Models;

public sealed class SmsNotification
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string Message { get; set; }

    [Required] [StringLength(255)] public string RecipientPhoneNumber { get; set; }
    [Required] public string RecipientUserId { get; set; }
    public ApplicationUser RecipientUser { get; set; }

    [Required] [StringLength(255)] public string Sender { get; set; }

    [Required] public bool IsSent { get; set; }

    [Required] public DateTimeOffset? DateSent { get; set; }
    
    public int? ResponseStatusCode { get; set; }
    public string? ResponseStatus { get; set; }
    public string? ResponseErrorDescription { get; set; }
    public string? ResponsePayload { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}