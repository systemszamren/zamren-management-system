using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;

namespace zamren_management_system.Areas.Admin.Controllers.Security;

[Authorize(Roles = "ADMIN")]
[Area("Admin")]
public class OfficeController : Controller
{
    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public IActionResult Edit()
    {
        return View();
    }

    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public IActionResult ManageUsers()
    {
        return View();
    }

    [HttpGet("/Admin/Office/ManageUsers/EditTenure/{id}")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    // ReSharper disable once UnusedParameter.Global
    public IActionResult EditTenure(string id)
    {
        return View();
    }
}