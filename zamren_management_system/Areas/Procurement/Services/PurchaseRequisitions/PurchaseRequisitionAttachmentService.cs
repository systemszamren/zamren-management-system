using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Services.PurchaseRequisition;

public class PurchaseRequisitionAttachmentService : IPurchaseRequisitionAttachmentService
{
    private readonly AuthContext _context;
    private readonly ILogger<PurchaseRequisitionAttachmentService> _logger;

    public PurchaseRequisitionAttachmentService(AuthContext context,
        ILogger<PurchaseRequisitionAttachmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(
        PurchaseRequisitionAttachment purchaseRequisitionAttachment)
    {
        try
        {
            await _context.PurchaseRequisitionAttachments.AddAsync(purchaseRequisitionAttachment);
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

    public async Task<IdentityResult> UpdateAsync(
        PurchaseRequisitionAttachment purchaseRequisitionAttachment)
    {
        try
        {
            _context.PurchaseRequisitionAttachments.Update(purchaseRequisitionAttachment);
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

    public async Task<IdentityResult> DeleteAsync(
        PurchaseRequisitionAttachment purchaseRequisitionAttachment)
    {
        try
        {
            _context.PurchaseRequisitionAttachments.Remove(purchaseRequisitionAttachment);
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

    public async Task<PurchaseRequisitionAttachment?> FindByIdAsync(
        string purchaseRequisitionAttachmentId)
    {
        return await _context.PurchaseRequisitionAttachments
            .FirstOrDefaultAsync(pra => pra.Id == purchaseRequisitionAttachmentId);
    }

    public async Task<List<PurchaseRequisitionAttachment>> FindAllAsync()
    {
        return await _context.PurchaseRequisitionAttachments.ToListAsync();
    }

    public async Task<ICollection<PurchaseRequisitionAttachment>> FindByPurchaseRequisitionIdAsync(
        string purchaseRequisitionId)
    {
        return await _context.PurchaseRequisitionAttachments
            .Where(pra => pra.PurchaseRequisitionId == purchaseRequisitionId)
            .Include(pra => pra.SystemAttachment)
            .ToListAsync();
    }
}