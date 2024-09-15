using zamren_management_system.Areas.Common.Dto;

namespace zamren_management_system.Areas.Procurement.Dto.PurchaseRequisitions;

public class PurchaseRequisitionAttachmentDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public PurchaseRequisitionDto? PurchaseRequisitionDto { get; set; }
    public SystemAttachmentDto SystemAttachment { get; set; }
}