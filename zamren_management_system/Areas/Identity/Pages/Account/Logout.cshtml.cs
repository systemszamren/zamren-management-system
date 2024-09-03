// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        //get logged out user to redirect to login page
        var user = await _signInManager.UserManager.GetUserAsync(User);
        if (user != null)
        {
            user.RecentActivity = Status.LoggedOut.ToString();
            user.LastLogoutDate = DateTimeOffset.UtcNow;
            user.ModifiedDate = DateTimeOffset.UtcNow;
            user.ModifiedByUserId = user.Id;
            await _signInManager.UserManager.UpdateAsync(user);
        }

        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }

        // This needs to be a redirect so that the browser performs a new
        // request and the identity for the user gets updated.
        return RedirectToPage();
    }
}