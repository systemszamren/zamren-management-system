// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using zamren_management_system.Areas.Security.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Services;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Identity.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IEmailSender _emailSender;
    private readonly Util _util;
    private readonly IPasswordHistoryService _passwordHistoryService;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, IPasswordHistoryService passwordHistoryService, IEmailTemplate emailTemplate, Util util, IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _passwordHistoryService = passwordHistoryService;
        _emailTemplate = emailTemplate;
        _util = util;
        _emailSender = emailSender;
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

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        public string Code { get; set; }
    }

    public async Task<IActionResult> OnGet(string code = null)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        Input = new InputModel
        {
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
        };

        // Log out the user
        await _signInManager.SignOutAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var passwordAlreadyUsed = await _userManager.CheckPasswordAsync(user, Input.Password);
        if (passwordAlreadyUsed)
        {
            ModelState.AddModelError(string.Empty,
                "Invalid email or password.");
            return Page();
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
        {
            var monthsUntilPasswordExpires =
                Convert.ToInt32(_configuration["SystemVariables:MonthsUntilPasswordExpires"]);
            var currentUserId = _userManager.GetUserId(User);
            var currentDateTimeOffset = DateTimeOffset.UtcNow;
            await _userManager.ConfirmAccountEmailAsync(user, currentUserId);
            await _userManager.UnlockAccountAsync(user, currentUserId);
            user.LastSuccessfulPasswordChangeDate = currentDateTimeOffset;
            user.PasswordExpiryDate = currentDateTimeOffset.AddMonths(monthsUntilPasswordExpires);
            user.ChangePasswordOnNextLogin = false;
            user.RecentActivity = Status.PasswordChanged.ToString();
            user.ModifiedDate = currentDateTimeOffset;
            user.ModifiedByUserId = currentUserId;
            await _userManager.UpdateAsync(user);

            //save password history
            if (string.IsNullOrEmpty(user.PasswordHash)) return RedirectToPage("./ResetPasswordConfirmation");

            var passwordHistories = await _passwordHistoryService.GetPasswordHistories(user.Id);
            foreach (var passwordHistory in passwordHistories)
            {
                passwordHistory.PasswordExpiryDate = currentDateTimeOffset;
                passwordHistory.Status = Status.Expired.ToString();
                passwordHistory.ModifiedByUserId = currentUserId;
                passwordHistory.ModifiedDate = currentDateTimeOffset;
                await _passwordHistoryService.UpdateAsync(passwordHistory);
            }

            await _passwordHistoryService.CreateAsync(new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                PasswordCreatedDate = currentDateTimeOffset,
                PasswordExpiryDate = (DateTimeOffset)user.PasswordExpiryDate,
                CreatedByUserId = user.Id,
                CreatedDate = currentDateTimeOffset
            });

            //send email notification to user about password change
            var body = _emailTemplate.PasswordChanged(
                user.FirstName,
                _util.HideEmailAddress(user.Email)
            );

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Password Changed",
                body,
                _userManager.GetUserId(User)
            );

            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}