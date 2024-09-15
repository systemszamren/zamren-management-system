using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;

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
}