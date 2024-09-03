using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;

public interface IPurchaseRequisitionRequestService
{
    public Task<IdentityResult> CreateAsync(PurchaseRequisitionRequest purchaseRequisitionRequest);

    public Task<IdentityResult> UpdateAsync(PurchaseRequisitionRequest purchaseRequisitionRequest);

    public Task<IdentityResult> DeleteAsync(PurchaseRequisitionRequest purchaseRequisitionRequest);

    public Task<PurchaseRequisitionRequest?> FindByIdAsync(string purchaseRequisitionRequestId);

    public Task<List<PurchaseRequisitionRequest>> FindAllAsync();

    public Task<ICollection<PurchaseRequisitionRequest>> FindByDepartmentAsync(string departmentId);

    public Task<ICollection<PurchaseRequisitionRequest>> FindByRequestingOfficerAsync(
        string requestingOfficerUserId);

    public Task<PurchaseRequisitionRequest?> FindByReferenceAsync(string reference);
}