using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;

public interface IPurchaseRequisitionAttachmentService
{
    public Task<IdentityResult> CreateAsync(PurchaseRequisitionAttachment purchaseRequisitionAttachment);

    public Task<IdentityResult> UpdateAsync(PurchaseRequisitionAttachment purchaseRequisitionAttachment);

    public Task<IdentityResult> DeleteAsync(PurchaseRequisitionAttachment purchaseRequisitionAttachment);

    public Task<PurchaseRequisitionAttachment?> FindByIdAsync(string purchaseRequisitionAttachmentId);

    public Task<List<PurchaseRequisitionAttachment>> FindAllAsync();

    public Task<ICollection<PurchaseRequisitionAttachment>> FindByPurchaseRequisitionIdAsync(
        string purchaseRequisitionId);
}