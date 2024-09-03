using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace zamren_management_system.Areas.Client.Controllers.Dashboard;

[Authorize(Roles = "CLIENT")]
// [Route("client")]
[Area("Client")]
public class DashboardController : Controller
{
    // [Route("dashboard")]
    public IActionResult Index()
    {
        return View();
    }
}