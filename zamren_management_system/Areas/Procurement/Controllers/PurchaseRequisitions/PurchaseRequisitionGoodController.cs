using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace zamren_management_system.Areas.Procurement.Controllers.PurchaseRequisition;

[Authorize(Roles = "EMPLOYEE")]
// [Route("procurement")]
[Area("Procurement")]
public class PurchaseRequisitionGoodController : Controller
{
    // [HttpGet("/Procurement/PurchaseRequisition/InitiatePurchaseRequisition")]
    public IActionResult InitiatePurchaseRequisition([FromQuery] string? reference)
    {
        return View();
    }
}