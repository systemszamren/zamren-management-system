// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IUtil _util;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
        IConfiguration configuration, IEmailTemplate emailTemplate, IUtil util)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _emailTemplate = emailTemplate;
        _util = util;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        
        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user == null)
        {
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        if (user.IsScheduledForDeletion && user.AccountDeletionScheduledDate != null)
        {
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code },
            protocol: Request.Scheme);
        
        var body = _emailTemplate.ResetPassword(
            user.FirstName,
            _util.HideEmailAddress(Input.Email),
            HtmlEncoder.Default.Encode(callbackUrl!),
            Convert.ToInt32(expireTokenInHours)
        );

        await _emailSender.SendEmailAsync(
            Input.Email,
            "Reset Password",
            body,
            user.Id
        );

        return RedirectToPage("./ForgotPasswordConfirmation");

    }
}