using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;

public interface IPurchaseRequisitionGoodService
{
    Task<IdentityResult> CreateAsync(PurchaseRequisitionGood purchaseRequisitionGood);
    Task<IdentityResult> UpdateAsync(PurchaseRequisitionGood purchaseRequisitionGood);
    Task<IdentityResult> DeleteAsync(PurchaseRequisitionGood purchaseRequisitionGood);
    Task<PurchaseRequisitionGood?> FindByIdAsync(string purchaseRequisitionGoodId);
    Task<ICollection<PurchaseRequisitionGood>> FindByPurchaseRequisitionIdAsync(string purchaseRequisitionId);
}