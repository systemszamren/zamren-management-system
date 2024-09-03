// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly ICypherService _cypherService;
    private readonly IConfiguration _configuration;
    private readonly IUserDetailService _userDetailService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IUtil _util;

    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<ExternalLoginModel> logger, IEmailSender emailSender, IConfiguration configuration, ICypherService cypherService, IUserDetailService userDetailService, IEmailTemplate emailTemplate, IUtil util)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
        _emailSender = emailSender;
        _configuration = configuration;
        _cypherService = cypherService;
        _userDetailService = userDetailService;
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
    public string ProviderDisplayName { get; set; }

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
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
            isPersistent: false, bypassTwoFactor: true);

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        if (result.IsNotAllowed)
        {
            return RedirectToPage("./Login");
        }

        if (result.Succeeded)
        {
            var user = await _signInManager.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("./Login");
            }

            if (user.IsScheduledForDeletion && user.AccountDeletionScheduledDate != null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("./AccountDeletion");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("./Lockout");
            }

            //createdByUserId
            var createdByUserId = user!.Id;

            //update last successful login date
            user.LastSuccessfulLoginDate = DateTimeOffset.UtcNow;
            user.RecentActivity = Status.LoggedInUsingGoogle.ToString();
            user.ModifiedByUserId = createdByUserId;
            user.ModifiedDate = DateTimeOffset.UtcNow;
            await _signInManager.UserManager.UpdateAsync(user);

            //redirect to admin dashboard if user has admin role else redirect to client dashboard
            // return Redirect(hasAdminRole ? "/Admin/Dashboard" : "/Client/Dashboard");


            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity!.Name,
                info.LoginProvider);

            return LocalRedirect(returnUrl);
        }

        // If the user does not have an account, then ask the user to create an account.
        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            Input = new InputModel
            {
                Email = info.Principal.FindFirstValue(ClaimTypes.Email)
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            //get first name and last name from external login provider
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            var monthsUntilPasswordExpires =
                Convert.ToInt32(_configuration["SystemVariables:MonthsUntilPasswordExpires"]);

            var currentDateTimeOffset = DateTimeOffset.UtcNow;
            user.FirstName = firstName ?? user!.Email!.Split('@')[0];
            user.LastName = lastName ?? "User";
            user.AccountCreatedDate = currentDateTimeOffset;
            user.CreatedByUserId = user.Id;
            user.IsEmployee = false;
            user.CanActionWkfTasks = false;
            user.CreatedDate = currentDateTimeOffset;
            user.PasswordExpiryDate = currentDateTimeOffset.AddMonths(monthsUntilPasswordExpires);
            user.ChangePasswordOnNextLogin = false;
            user.RecentActivity = Status.AccountCreatedUsingGoogle.ToString();
            // user.LockoutEnabled = true;

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                //create user detail
                await _userDetailService.CreateAsync(new UserDetail
                {
                    UserId = user.Id,
                    ProfileCompletionPercentage = 13,
                    CreatedDate =  currentDateTimeOffset,
                    CreatedByUserId = user.Id!
                });
                
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    var userId = await _userManager.GetUserIdAsync(user);
                    userId = _cypherService.Encrypt(userId);
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
                        _util.HideEmailAddress(Input.Email),
                        HtmlEncoder.Default.Encode(callbackUrl!),
                        Convert.ToInt32(expireTokenInHours)
                    );

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirm Email",
                        body,
                        user.Id
                    );

                    // If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Input.Email });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}