using zamren_management_system.Areas.Common.Dto;

namespace zamren_management_system.Areas.Procurement.Dto.PurchaseRequisition;

public class PurchaseRequisitionRequestAttachmentDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public PurchaseRequisitionRequestDto? PurchaseRequisitionRequest { get; set; }
    public SystemAttachmentDto SystemAttachment { get; set; }

    
}