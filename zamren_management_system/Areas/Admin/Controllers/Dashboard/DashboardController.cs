using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;

namespace zamren_management_system.Areas.Admin.Controllers.Dashboard;

[Authorize(Roles = "ADMIN")]
[Area("Admin")]
public class DashboardController : Controller
{
    [HasPrivilege(PrivilegeConstant.DASH_ACCESS_ADMIN_DASHBOARD)]
    public IActionResult Index()
    {
        return View();
    }
}