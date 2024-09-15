using System.ComponentModel.DataAnnotations;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Procurement.Models;

public sealed class PurchaseRequisition
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string Reference { get; set; }

    [Required] public string RequestingOfficerUserId { get; set; }
    public ApplicationUser RequestingOfficerUser { get; set; }
    
    public string? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    
    public string? BranchId { get; set; }
    public Branch? Branch { get; set; }
    
    public string? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? OfficeId { get; set; }
    public Office? Office { get; set; }
    
    /*[Required] [StringLength(255)] public string ItemDescription { get; set; }

    [Required] public int Quantity { get; set; }

    [Required] public decimal EstimatedCost { get; set; }*/

    [Required] [StringLength(255)] public string Justification { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedBy { get; set; }

    [Required] public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedByUserId { get; set; }

    public ApplicationUser? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public IEnumerable<PurchaseRequisitionAttachment>? PurchaseRequisitionAttachments { get; set; }
    public IEnumerable<PurchaseRequisitionGood>? PurchaseRequisitionGoods { get; set; }
    public IEnumerable<PurchaseRequisitionService>? PurchaseRequisitionServices { get; set; }
}