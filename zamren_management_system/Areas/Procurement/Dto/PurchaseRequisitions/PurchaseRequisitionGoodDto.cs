using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Procurement.Dto.PurchaseRequisitions;

public class PurchaseRequisitionGoodDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public PurchaseRequisitionDto? PurchaseRequisition { get; set; }
    public string? ItemDescription { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public UserDto? VendorUser { get; set; }
    public SystemAttachmentDto? SystemAttachment { get; set; }

}