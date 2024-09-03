using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.ViewModels;

namespace zamren_management_system.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        const string adminUrl = "~/Admin/Dashboard";
        const string backofficeUrl = "~/Backoffice/Dashboard";
        const string clientUrl = "~/Client/Dashboard";

        //redirect per role to the correct dashboard else to the login page
        var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
        var adminRoleName = _configuration["DefaultUserRoles:AdminRole"] ?? string.Empty;
        var employeeRoleName = _configuration["DefaultUserRoles:EmployeeRole"] ?? string.Empty;

        //if the user is logged in and authenticated and has a adminRoleName role
        if (User.Identity is { IsAuthenticated: true } && User.IsInRole(adminRoleName))
            return Redirect(adminUrl);

        //if the user is logged in and authenticated and has a employeeRoleName role
        if (User.Identity is { IsAuthenticated: true } && User.IsInRole(employeeRoleName))
            return Redirect(backofficeUrl);

        //if the user is logged in and authenticated and has a clientRoleName role
        if (User.Identity is { IsAuthenticated: true } && User.IsInRole(clientRoleName))
            return Redirect(clientUrl);
        
        return View();
    }

    public IActionResult AboutUs()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("Home/SystemError/{statusCode:int}")]
    public IActionResult SystemError(int statusCode)
    {
        return View(new SystemErrorViewModel { StatusCode = statusCode });
    }
}