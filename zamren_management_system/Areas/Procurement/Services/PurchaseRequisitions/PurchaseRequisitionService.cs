using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;

namespace zamren_management_system.Areas.Procurement.Services.PurchaseRequisitions;

public class PurchaseRequisitionService : IPurchaseRequisitionService
{
    private readonly AuthContext _context;
    private readonly ILogger<PurchaseRequisitionService> _logger;

    public PurchaseRequisitionService(AuthContext context, ILogger<PurchaseRequisitionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(Models.PurchaseRequisition purchaseRequisition)
    {
        try
        {
            await _context.PurchaseRequisitions.AddAsync(purchaseRequisition);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(Models.PurchaseRequisition purchaseRequisition)
    {
        try
        {
            _context.PurchaseRequisitions.Update(purchaseRequisition);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(Models.PurchaseRequisition purchaseRequisition)
    {
        try
        {
            _context.PurchaseRequisitions.Remove(purchaseRequisition);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<Models.PurchaseRequisition?> FindByIdAsync(string purchaseRequisitionId)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .FirstOrDefaultAsync(pr => pr.Id == purchaseRequisitionId);
    }

    //FindAllAsync
    public async Task<List<Models.PurchaseRequisition>> FindAllAsync()
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .ToListAsync();
    }

    //FindByDepartmentAsync
    public async Task<ICollection<Models.PurchaseRequisition>> FindByDepartmentAsync(string departmentId)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .Where(pr => pr.DepartmentId == departmentId)
            .ToListAsync();
    }

    //FindByRequestingOfficerAsync
    public async Task<ICollection<Models.PurchaseRequisition>> FindByRequestingOfficerAsync(
        string requestingOfficerUserId)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .Where(pr => pr.RequestingOfficerUserId == requestingOfficerUserId)
            .ToListAsync();
    }

    //FindByReferenceAsync
    public async Task<Models.PurchaseRequisition?> FindByReferenceAsync(string reference)
    {
        return await _context.PurchaseRequisitions
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .FirstOrDefaultAsync(pr => pr.Reference == reference);
    }
}