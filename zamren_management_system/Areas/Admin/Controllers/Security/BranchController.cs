using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;

namespace zamren_management_system.Areas.Admin.Controllers.Security;

[Authorize(Roles = "ADMIN")]
[Area("Admin")]
public class BranchController : Controller
{
    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public IActionResult Edit()
    {
        return View();
    }
}