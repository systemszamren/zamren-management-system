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
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ICypherService _cypherService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IUtil _util;

    public EmailModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender, IConfiguration configuration, ICypherService cypherService, IEmailTemplate emailTemplate, IUtil util)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _cypherService = cypherService;
        _emailTemplate = emailTemplate;
        _util = util;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Display(Name = "Current Email")]
    public string Email { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

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
        [Display(Name = "New email")]
        public string NewEmail { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        var email = await _userManager.GetEmailAsync(user);
        Email = email;

        Input = new InputModel
        {
            NewEmail = email
        };

        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.NewEmail != email)
        {
            var userWithEmail = await _userManager.FindByEmailAsync(Input.NewEmail);
            if (userWithEmail != null)
            {
                StatusMessage = "A confirmation link has been sent to your new email. Please check your email.";
                return RedirectToPage();
            }


            var userId = await _userManager.GetUserIdAsync(user);
            userId = _cypherService.Encrypt(userId);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);

            var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId, email = Input.NewEmail, code },
                protocol: Request.Scheme);
            
            var body = _emailTemplate.ConfirmEmail(
                user.FirstName,
                _util.HideEmailAddress(Input.NewEmail),
                HtmlEncoder.Default.Encode(callbackUrl!),
                Convert.ToInt32(expireTokenInHours)
            );

            await _emailSender.SendEmailAsync(
                Input.NewEmail,
                "Confirm Email",
                body,
                user.Id
            );

            StatusMessage = "A confirmation link has been sent to your new email. Please check your email.";
            return RedirectToPage();
        }

        StatusMessage = "Your email is unchanged.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        userId = _cypherService.Encrypt(userId);
        var email = await _userManager.GetEmailAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId, code },
            protocol: Request.Scheme);
        
        var body = _emailTemplate.ConfirmEmail(
            user.FirstName,
            _util.HideEmailAddress(email),
            HtmlEncoder.Default.Encode(callbackUrl!),
            Convert.ToInt32(expireTokenInHours)
        );

        await _emailSender.SendEmailAsync(
            email!,
            "Confirm Email",
            body,
            user.Id
        );

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }
}