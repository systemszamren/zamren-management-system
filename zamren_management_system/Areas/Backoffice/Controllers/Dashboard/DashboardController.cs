using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace zamren_management_system.Areas.Backoffice.Controllers.Dashboard;

[Authorize(Roles = "EMPLOYEE")]
// [Route("backoffice")]
[Area("Backoffice")]
public class DashboardController : Controller
{
    // [Route("dashboard")]
    public IActionResult Index()
    {
        return View();
    }
}