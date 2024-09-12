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
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ICypherService _cypherService;
    private readonly IUserDetailService _userDetailService;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IUtil _util;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger, IEmailSender emailSender, IConfiguration configuration,
        ICypherService cypherService, IPasswordHistoryService passwordHistoryService,
        IUserDetailService userDetailService, IEmailTemplate emailTemplate, IUtil util)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _configuration = configuration;
        _cypherService = cypherService;
        _passwordHistoryService = passwordHistoryService;
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
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }


    public async Task OnGetAsync(string returnUrl = null)
    {
        if (true) Response.Redirect("/"); //REMOVE REGISTRATION PAGE
        
        // Redirect to home if already logged in
        if (User.Identity!.IsAuthenticated)
        {
            Response.Redirect("/");
        }

        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = CreateUser();

            var monthsUntilPasswordExpires =
                Convert.ToInt32(_configuration["SystemVariables:MonthsUntilPasswordExpires"]);

            var currentDatetime = DateTimeOffset.UtcNow;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.AccountCreatedDate = currentDatetime;
            user.CreatedByUserId = user.Id;
            user.IsEmployee = false;
            user.CanActionWkfTasks = false;
            user.CreatedDate = currentDatetime;
            user.PasswordExpiryDate = currentDatetime.AddMonths(monthsUntilPasswordExpires);
            user.ChangePasswordOnNextLogin = false;

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            //create user detail
            await _userDetailService.CreateAsync(new UserDetail
            {
                UserId = user.Id,
                ProfileCompletionPercentage = 13,
                CreatedDate = currentDatetime,
                CreatedByUserId = user.Id!
            });

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                //save password history
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    var passwordHistories = await _passwordHistoryService.GetPasswordHistories(user.Id);
                    foreach (var passwordHistory in passwordHistories)
                    {
                        passwordHistory.PasswordExpiryDate = currentDatetime;
                        passwordHistory.Status = Status.Expired.ToString();
                        passwordHistory.ModifiedByUserId = user.Id;
                        passwordHistory.ModifiedDate = currentDatetime;
                        await _passwordHistoryService.UpdateAsync(passwordHistory);
                    }

                    await _passwordHistoryService.CreateAsync(new PasswordHistory
                    {
                        UserId = user.Id,
                        PasswordHash = user.PasswordHash,
                        PasswordCreatedDate = currentDatetime,
                        PasswordExpiryDate = (DateTimeOffset)user.PasswordExpiryDate,
                        CreatedByUserId = user.Id,
                        CreatedDate = currentDatetime
                    });
                }

                var userId = await _userManager.GetUserIdAsync(user);
                userId = _cypherService.Encrypt(userId);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId, code, returnUrl },
                    protocol: Request.Scheme);

                var body = _emailTemplate.RegisterConfirmEmail(
                    user.FirstName,
                    _util.HideEmailAddress(Input.Email),
                    HtmlEncoder.Default.Encode(callbackUrl!),
                    Convert.ToInt32(expireTokenInHours)
                );

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Confirm Email",
                    body,
                    user.Id);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
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
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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