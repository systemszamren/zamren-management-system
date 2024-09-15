using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Procurement.Dto.PurchaseRequisitions;

namespace zamren_management_system.Areas.Procurement.Dto.PurchaseRequisition;

public class PurchaseRequisitionAttachmentDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public PurchaseRequisitionDto? PurchaseRequisitionDto { get; set; }
    public SystemAttachmentDto SystemAttachment { get; set; }
}