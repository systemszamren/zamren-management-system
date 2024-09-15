using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;

namespace zamren_management_system.Areas.Procurement.Services.PurchaseRequisitions;

public class PurchaseRequisitionServiceService : IPurchaseRequisitionServiceService
{
    private readonly AuthContext _context;
    private readonly ILogger<PurchaseRequisitionServiceService> _logger;

    public PurchaseRequisitionServiceService(AuthContext context, ILogger<PurchaseRequisitionServiceService> logger)
    {
        _context = context;
        _logger = logger;
    }
}