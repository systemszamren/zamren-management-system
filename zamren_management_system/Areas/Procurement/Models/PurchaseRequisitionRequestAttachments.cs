using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Procurement.Models;

public sealed class PurchaseRequisitionRequestAttachment
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string PurchaseRequisitionRequestId { get; set; }
    public PurchaseRequisitionRequest PurchaseRequisitionRequest { get; set; }

    [Required] public string AttachmentId { get; set; }
    public SystemAttachment SystemAttachment { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}