namespace zamren_management_system.Areas.Procurement.Dto.PurchaseRequisitions;

public class PurchaseRequisitionDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public string? Reference { get; set; }
    public string? OrganizationId { get; set; }
    public string? BranchId { get; set; }
    public string? DepartmentId { get; set; }
    public string? OfficeId { get; set; }
    // public string? ItemDescription { get; set; }
    // public int? Quantity { get; set; }
    // public string? QuantityString { get; set; }
    // public decimal? EstimatedCost { get; set; }
    // public string? EstimatedCostString { get; set; }
    public string? Justification { get; set; }
    public List<PurchaseRequisitionGoodDto>? PurchaseRequisitionGoods { get; set; }
    public List<PurchaseRequisitionAttachmentDto>? PurchaseRequisitionAttachments { get; set; }
}