// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class ConfirmEmail2Model : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICypherService _cypherService;
    private readonly IUserDetailService _userDetailService;

    public ConfirmEmail2Model(UserManager<ApplicationUser> userManager, ICypherService cypherService,
        IUserDetailService userDetailService)
    {
        _userManager = userManager;
        _cypherService = cypherService;
        _userDetailService = userDetailService;
    }


    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string code, string emailType)
    {
        if (userId == null || code == null || emailType == null)
        {
            return RedirectToPage("/Index");
        }

        userId = _cypherService.Decrypt(userId);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        //get user details
        var userDetails = await _userDetailService.FindByUserIdAsync(user.Id);
        if (userDetails == null)
        {
            return NotFound($"Unable to load user details with ID '{userId}'.");
        }

        // code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var result = await _userDetailService.ConfirmEmailAsync(user, code, emailType);
        if (result.Succeeded)
        {
            if (emailType == EmailVerificationType.Alternative.ToString())
            {
                StatusMessage = "Thank you for confirming your alternative email address.";
            }
            else if (emailType == EmailVerificationType.NextOfKin.ToString())
            {
                StatusMessage =
                    $"You have successfully confirmed as the Next of Kin to {user.FirstName} {user.LastName}.";
            }
            else
            {
                StatusMessage = "Thank you for confirming your email.";
            }
        }
        else
            StatusMessage = "Error confirming your request. Token might have expired. Try resending the token.";

        return Page();
    }
}