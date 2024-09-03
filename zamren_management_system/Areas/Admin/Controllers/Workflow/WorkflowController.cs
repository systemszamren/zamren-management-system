using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;

namespace zamren_management_system.Areas.Admin.Controllers.Workflow;

[Authorize(Roles = "ADMIN")]
[Area("Admin")]
public class WorkflowController : Controller
{
    [HttpGet]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public IActionResult Tasks()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public IActionResult Task()
    {
        return View();
    }
}