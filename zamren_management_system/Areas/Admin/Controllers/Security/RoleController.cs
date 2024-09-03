using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;

namespace zamren_management_system.Areas.Admin.Controllers.Security;

[Authorize(Roles = "ADMIN")]
[Area("Admin")]
public class RoleController : Controller
{
    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public IActionResult Edit()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public IActionResult ManageUsers()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public IActionResult ManagePrivileges()
    {
        return View();
    }

    [HttpGet("/Admin/Role/ManageUsers/EditTenure/{id}")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public IActionResult EditTenure(string id)
    {
        return View();
    }
}