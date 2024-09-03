using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Models;

public sealed class SystemNotification
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string RecipientUserId { get; set; }

    public ApplicationUser Recipient { get; set; }

    [Required] [StringLength(255)] public string Subject { get; set; }

    [Required] [DataType(DataType.Text)] public string Body { get; set; }

    [Required] public bool IsRead { get; set; } = false;

    public DateTimeOffset? DateRead { get; set; }

    [Required] public DateTimeOffset DatePosted { get; set; }

    [Required] public string NotificationType { get; set; } // Type of notification (e.g., "Alert", "Reminder")

    public string? ActionUrl { get; set; } // URL for the user to take action on the notification
    public string? ActionUrlText { get; set; } // Text for the action URL

    [Required] public int Priority { get; set; } //e.g., 1 = High, 2 = Medium, 3 = Low

    [Required] [StringLength(255)] public string Status { get; set; } = Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public IEnumerable<SystemNotificationAttachment>? SystemNotificationAttachments { get; set; }
}