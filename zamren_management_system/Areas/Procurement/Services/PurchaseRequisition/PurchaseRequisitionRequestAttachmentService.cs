using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Services.PurchaseRequisition;

public class PurchaseRequisitionRequestAttachmentService : IPurchaseRequisitionRequestAttachmentService
{
    private readonly AuthContext _context;
    private readonly ILogger<PurchaseRequisitionRequestAttachmentService> _logger;

    public PurchaseRequisitionRequestAttachmentService(AuthContext context,
        ILogger<PurchaseRequisitionRequestAttachmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(
        PurchaseRequisitionRequestAttachment purchaseRequisitionRequestAttachment)
    {
        try
        {
            await _context.PurchaseRequisitionRequestAttachments.AddAsync(purchaseRequisitionRequestAttachment);
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
        PurchaseRequisitionRequestAttachment purchaseRequisitionRequestAttachment)
    {
        try
        {
            _context.PurchaseRequisitionRequestAttachments.Update(purchaseRequisitionRequestAttachment);
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
        PurchaseRequisitionRequestAttachment purchaseRequisitionRequestAttachment)
    {
        try
        {
            _context.PurchaseRequisitionRequestAttachments.Remove(purchaseRequisitionRequestAttachment);
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

    public async Task<PurchaseRequisitionRequestAttachment?> FindByIdAsync(
        string purchaseRequisitionRequestAttachmentId)
    {
        return await _context.PurchaseRequisitionRequestAttachments
            .FirstOrDefaultAsync(pra => pra.Id == purchaseRequisitionRequestAttachmentId);
    }

    public async Task<List<PurchaseRequisitionRequestAttachment>> FindAllAsync()
    {
        return await _context.PurchaseRequisitionRequestAttachments.ToListAsync();
    }

    public async Task<ICollection<PurchaseRequisitionRequestAttachment>> FindByPurchaseRequisitionRequestIdAsync(
        string purchaseRequisitionRequestId)
    {
        return await _context.PurchaseRequisitionRequestAttachments
            .Where(pra => pra.PurchaseRequisitionRequestId == purchaseRequisitionRequestId)
            .Include(pra => pra.SystemAttachment)
            .ToListAsync();
    }
}