using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Procurement.Models;

public sealed class PurchaseRequisitionService
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string PurchaseRequisitionId { get; set; }
    public PurchaseRequisition PurchaseRequisition { get; set; }

    [Required] [StringLength(255)] public string Description { get; set; }

    [Required] public decimal EstimatedCost { get; set; }

    public string? VendorUserId { get; set; }
    public ApplicationUser? VendorUser { get; set; }

    public DateTimeOffset? ServiceStartDate { get; set; }
    public DateTimeOffset? ServiceEndDate { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}