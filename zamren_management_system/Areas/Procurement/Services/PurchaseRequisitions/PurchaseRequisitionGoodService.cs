using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;
using zamren_management_system.Areas.Procurement.Models;

namespace zamren_management_system.Areas.Procurement.Services.PurchaseRequisitions;

public class PurchaseRequisitionGoodService : IPurchaseRequisitionGoodService
{
    private readonly AuthContext _context;
    private readonly ILogger<PurchaseRequisitionGoodService> _logger;

    public PurchaseRequisitionGoodService(AuthContext context, ILogger<PurchaseRequisitionGoodService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(PurchaseRequisitionGood purchaseRequisitionGood)
    {
        try
        {
            await _context.PurchaseRequisitionGoods.AddAsync(purchaseRequisitionGood);
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

    public async Task<IdentityResult> UpdateAsync(PurchaseRequisitionGood purchaseRequisitionGood)
    {
        try
        {
            _context.PurchaseRequisitionGoods.Update(purchaseRequisitionGood);
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

    public async Task<IdentityResult> DeleteAsync(PurchaseRequisitionGood purchaseRequisitionGood)
    {
        try
        {
            _context.PurchaseRequisitionGoods.Remove(purchaseRequisitionGood);
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

    public async Task<PurchaseRequisitionGood?> FindByIdAsync(string purchaseRequisitionGoodId)
    {
        return await _context.PurchaseRequisitionGoods
            .FirstOrDefaultAsync(prg => prg.Id == purchaseRequisitionGoodId);
    }

    public async Task<List<PurchaseRequisitionGood>> FindAllAsync()
    {
        return await _context.PurchaseRequisitionGoods.ToListAsync();
    }

    //FindByPurchaseRequisitionIdAsync
    public async Task<ICollection<PurchaseRequisitionGood>> FindByPurchaseRequisitionIdAsync(
        string purchaseRequisitionId)
    {
        return await _context.PurchaseRequisitionGoods
            .Where(prg => prg.PurchaseRequisitionId == purchaseRequisitionId)
            .ToListAsync();
    }
}