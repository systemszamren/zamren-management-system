using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Models;
using zamren_management_system.Areas.Security.Models;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Common.Models;

public sealed class SystemAttachment
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string SystemFileName { get; set; }

    [Required] [StringLength(255)] public string OriginalFileName { get; set; }

    [Required] [StringLength(255)] public string CustomFileName { get; set; }

    [Required] [DataType(DataType.Text)] public string FilePath { get; set; }

    public long FileSize { get; set; }

    [Required] [StringLength(255)] public string ContentType { get; set; }

    [Required] [StringLength(255)] public string FileExtension { get; set; }

    [Required] [StringLength(255)] public string UploadedByUserId { get; set; }
    public ApplicationUser UploadedBy { get; set; }

    [Required] public DateTimeOffset DateUploaded { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public UserDetail? ProfilePictureAttachmentUser { get; set; }
    public UserDetail? IdentityAttachmentUser { get; set; }
    public UserDetail? ProofOfResidencyAttachmentUser { get; set; }
    public UserDetail? NextOfKinIdentityAttachmentUser { get; set; }
    public UserDetail? NextOfKinProofOfResidencyAttachmentUser { get; set; }
    public IEnumerable<EmailNotificationAttachment>? EmailNotificationAttachments { get; set; }
    public IEnumerable<WkfTaskAttachment>? WkfTaskAttachments { get; set; }
    public IEnumerable<SystemNotificationAttachment>? SystemNotificationAttachments { get; set; }
    public IEnumerable<PurchaseRequisitionAttachment>? PurchaseRequisitionAttachments { get; set; }
}