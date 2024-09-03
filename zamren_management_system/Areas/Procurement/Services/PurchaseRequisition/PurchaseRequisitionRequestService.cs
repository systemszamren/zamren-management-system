using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Services.PurchaseRequisition;

public class PurchaseRequisitionRequestService : IPurchaseRequisitionRequestService
{
    private readonly AuthContext _context;
    private readonly ILogger<PurchaseRequisitionRequestService> _logger;

    public PurchaseRequisitionRequestService(AuthContext context, ILogger<PurchaseRequisitionRequestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(PurchaseRequisitionRequest purchaseRequisitionRequest)
    {
        try
        {
            await _context.PurchaseRequisitionRequests.AddAsync(purchaseRequisitionRequest);
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

    public async Task<IdentityResult> UpdateAsync(PurchaseRequisitionRequest purchaseRequisitionRequest)
    {
        try
        {
            _context.PurchaseRequisitionRequests.Update(purchaseRequisitionRequest);
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

    public async Task<IdentityResult> DeleteAsync(PurchaseRequisitionRequest purchaseRequisitionRequest)
    {
        try
        {
            _context.PurchaseRequisitionRequests.Remove(purchaseRequisitionRequest);
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

    public async Task<PurchaseRequisitionRequest?> FindByIdAsync(string purchaseRequisitionId)
    {
        return await _context.PurchaseRequisitionRequests
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .FirstOrDefaultAsync(pr => pr.Id == purchaseRequisitionId);
    }

    //FindAllAsync
    public async Task<List<PurchaseRequisitionRequest>> FindAllAsync()
    {
        return await _context.PurchaseRequisitionRequests
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .ToListAsync();
    }

    //FindByDepartmentAsync
    public async Task<ICollection<PurchaseRequisitionRequest>> FindByDepartmentAsync(string departmentId)
    {
        return await _context.PurchaseRequisitionRequests
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
    public async Task<ICollection<PurchaseRequisitionRequest>> FindByRequestingOfficerAsync(
        string requestingOfficerUserId)
    {
        return await _context.PurchaseRequisitionRequests
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
    public async Task<PurchaseRequisitionRequest?> FindByReferenceAsync(string reference)
    {
        return await _context.PurchaseRequisitionRequests
            .Include(pr => pr.RequestingOfficerUser)
            .Include(pr => pr.Organization)
            .Include(pr => pr.Branch)
            .Include(pr => pr.Department)
            .Include(pr => pr.Office)
            .Include(pr => pr.CreatedBy)
            .FirstOrDefaultAsync(pr => pr.Reference == reference);
    }
}