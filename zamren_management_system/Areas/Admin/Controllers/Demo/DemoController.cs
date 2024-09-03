using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace zamren_management_system.Areas.Admin.Controllers.Demo;

[Authorize(Roles = "ADMIN")]
[Area("Admin")]
public class DemoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}