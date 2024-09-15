using Microsoft.AspNetCore.Identity;

namespace zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;

public interface IPurchaseRequisitionService
{
    public Task<IdentityResult> CreateAsync(Models.PurchaseRequisition purchaseRequisition);

    public Task<IdentityResult> UpdateAsync(Models.PurchaseRequisition purchaseRequisition);

    public Task<IdentityResult> DeleteAsync(Models.PurchaseRequisition purchaseRequisition);

    public Task<Models.PurchaseRequisition?> FindByIdAsync(string purchaseRequisitionId);

    public Task<List<Models.PurchaseRequisition>> FindAllAsync();

    public Task<ICollection<Models.PurchaseRequisition>> FindByDepartmentAsync(string departmentId);

    public Task<ICollection<Models.PurchaseRequisition>> FindByRequestingOfficerAsync(
        string requestingOfficerUserId);

    public Task<Models.PurchaseRequisition?> FindByReferenceAsync(string reference);
}