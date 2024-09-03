using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Models;

public sealed class EmailNotificationAttachment
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    public string AttachmentId { get; set; }

    public SystemAttachment SystemAttachment { get; set; }

    public string EmailNotificationId { get; set; }

    public EmailNotification EmailNotification { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}