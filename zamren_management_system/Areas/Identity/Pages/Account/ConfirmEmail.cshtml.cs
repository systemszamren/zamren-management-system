// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using zamren_management_system.Areas.Security.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IUserRoleService _userRoleService;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ICypherService _cypherService;

    public ConfirmEmailModel(UserManager<ApplicationUser> userManager, IConfiguration configuration,
        IUserRoleService userRoleService, RoleManager<ApplicationRole> roleManager, ICypherService cypherService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _userRoleService = userRoleService;
        _roleManager = roleManager;
        _cypherService = cypherService;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        userId = _cypherService.Decrypt(userId);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            await _userManager.ConfirmAccountEmailAsync(user, _userManager.GetUserId(User));
            await _userManager.UnlockAccountAsync(user, _userManager.GetUserId(User));

            // Add default role to user
            var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
            var role = await _roleManager.FindByNameAsync(clientRoleName);

            //check if user is already in role
            if (role != null)
            { 
                if (!await _userManager.IsInRoleAsync(user, role.Name!))
                    await _userRoleService.CreateAsync(new ApplicationUserRole
                    {
                        UserId = user.Id,
                        RoleId = role!.Id,
                        CreatedByUserId = user.Id,
                        CreatedDate = DateTimeOffset.UtcNow,
                        StartDate = DateTimeOffset.UtcNow,
                        EndDate = DateTimeOffset.MaxValue
                    });
            }

            StatusMessage = "Thank you for confirming your email. Please go ahead and Login.";
        }
        else
        {
            StatusMessage = "Error confirming your email. Token might have expired. Please try again.";
        }

        return Page();
    }
}