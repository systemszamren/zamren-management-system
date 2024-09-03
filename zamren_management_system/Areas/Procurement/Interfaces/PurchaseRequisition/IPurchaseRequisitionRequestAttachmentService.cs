using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;

public interface IPurchaseRequisitionRequestAttachmentService
{
    public Task<IdentityResult> CreateAsync(PurchaseRequisitionRequestAttachment purchaseRequisitionRequestAttachment);

    public Task<IdentityResult> UpdateAsync(PurchaseRequisitionRequestAttachment purchaseRequisitionRequestAttachment);

    public Task<IdentityResult> DeleteAsync(PurchaseRequisitionRequestAttachment purchaseRequisitionRequestAttachment);

    public Task<PurchaseRequisitionRequestAttachment?> FindByIdAsync(string purchaseRequisitionRequestAttachmentId);

    public Task<List<PurchaseRequisitionRequestAttachment>> FindAllAsync();

    public Task<ICollection<PurchaseRequisitionRequestAttachment>> FindByPurchaseRequisitionRequestIdAsync(
        string purchaseRequisitionRequestId);
}