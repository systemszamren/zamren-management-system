// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<LoginModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IUtil _util;

    public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger,
        UserManager<ApplicationUser> userManager, IEmailSender emailSender, IConfiguration configuration,
        IEmailTemplate emailTemplate, IUtil util)
    {
        _signInManager = signInManager;
        _logger = logger;
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
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

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

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        if (User.Identity!.IsAuthenticated)
        {
            Response.Redirect("/");
        }

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid) return Page();

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe,
            true);
        if (result.Succeeded)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);

            if (user != null)
            {
                //if scheduled for deletion, redirect to account deletion page
                if (user.IsScheduledForDeletion && user.AccountDeletionScheduledDate != null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("./AccountDeletion");
                }

                //if account is locked, redirect to account locked page
                if ((user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow) || user.LockoutEnabled)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("./Lockout");
                }

                //if password expired, redirect to password reset page
                if (user.PasswordExpiryDate != null && user.PasswordExpiryDate <= DateTimeOffset.UtcNow)
                {
                    user.RecentActivity = Status.PasswordExpired.ToString();
                    user.ChangePasswordOnNextLogin = true;
                    user.ModifiedByUserId = user.Id;
                    user.ModifiedDate = DateTimeOffset.UtcNow;
                    await _signInManager.UserManager.UpdateAsync(user);

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
                        "Password Expired",
                        body,
                        user.Id
                    );

                    await _signInManager.SignOutAsync();
                    return RedirectToPage("./ExpiredPassword");
                }

                user.LastSuccessfulLoginDate = DateTimeOffset.UtcNow;
                user.RecentActivity = Status.LoggedInUsingPassword.ToString();
                user.ModifiedByUserId = user.Id;
                user.ModifiedDate = DateTimeOffset.UtcNow;
                await _signInManager.UserManager.UpdateAsync(user);

                _logger.LogInformation("User logged in.");

                //redirect to returnUrl
                return LocalRedirect(returnUrl);
            }
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Incorrect username or password.");
        return Page();

        // If we got this far, something failed, redisplay form
    }
}