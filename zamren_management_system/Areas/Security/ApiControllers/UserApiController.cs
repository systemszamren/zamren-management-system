using System.Text;
using System.Text.Encodings.Web;
using zamren_management_system.Areas.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Area("Security")]
[Route("api/security/user")]
public class UserApiController : ControllerBase
{
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<UserApiController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ICypherService _cypherService;
    private readonly IUtil _util;
    private readonly ISystemAttachmentService _systemAttachmentService;
    private readonly IUserDetailService _userDetailService;
    private readonly IUserService _userService;
    private readonly ISystemAttachmentManager _systemAttachmentManager;
    private readonly ISmsSender _smsSender;
    private readonly IOtpService _otpService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IUserRoleService _userRoleService;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserApiController(UserManager<ApplicationUser> userManager, IDatatableService datatableService,
        ILogger<UserApiController> logger, IConfiguration configuration,
        IEmailSender emailSender, ICypherService cypherService, IPasswordHistoryService passwordHistoryService,
        IUtil util, IUserDetailService userDetailService, ISystemAttachmentService systemAttachmentService,
        IUserService userService, ISystemAttachmentManager systemAttachmentManager, ISmsSender smsSender,
        IOtpService otpService, IEmailVerificationService emailVerificationService, IEmailTemplate emailTemplate,
        IUserRoleService userRoleService, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _datatableService = datatableService;
        _logger = logger;
        _configuration = configuration;
        _emailSender = emailSender;
        _cypherService = cypherService;
        _passwordHistoryService = passwordHistoryService;
        _util = util;
        _userDetailService = userDetailService;
        _systemAttachmentService = systemAttachmentService;
        _userService = userService;
        _systemAttachmentManager = systemAttachmentManager;
        _smsSender = smsSender;
        _otpService = otpService;
        _emailVerificationService = emailVerificationService;
        _emailTemplate = emailTemplate;
        _userRoleService = userRoleService;
        _roleManager = roleManager;
    }

    /**
     * Get all users for the datatable
     */
    [HttpPost("get-users-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public IActionResult GetUsersDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var users = _userManager.Users;

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                userDto => userDto.FirstName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                           || userDto.LastName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                           || (userDto.Email != null &&
                               userDto.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                           || (userDto.PhoneNumber != null &&
                               userDto.PhoneNumber.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),
                userDto => userDto,
                users.ToList().Select((user, index) => new UserDto
                {
                    Counter = index + 1,
                    Id = _cypherService.Encrypt(user.Id),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsEmployee = user.IsEmployee,
                    Status = user.RecentActivity
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> Create()
    {
        try
        {
            var userDto = new UserDto
            {
                FirstName = Request.Form["firstName"].FirstOrDefault(),
                LastName = Request.Form["lastName"].FirstOrDefault(),
                Email = Request.Form["email"].FirstOrDefault()
            };
            var isEmployee = Request.Form["isEmployee"].FirstOrDefault();
            userDto.IsEmployee = !string.IsNullOrEmpty(isEmployee) && bool.Parse(isEmployee);

            //if the user is an employee, get Supervisor userId
            if ((bool)userDto.IsEmployee)
            {
                var supervisorUserId = Request.Form["supervisorUserId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(supervisorUserId))
                    userDto.SupervisorUserId = _cypherService.Decrypt(supervisorUserId);
            }

            var canActionWkfTasks = Request.Form["canActionWkfTasks"].FirstOrDefault();
            userDto.CanActionWkfTask = !string.IsNullOrEmpty(canActionWkfTasks) && bool.Parse(canActionWkfTasks);

            if (string.IsNullOrEmpty(userDto.Email))
                return Ok(new { success = false, message = "Email is required" });
            if (string.IsNullOrEmpty(userDto.FirstName))
                return Ok(new { success = false, message = "First Name is required" });
            if (string.IsNullOrEmpty(userDto.LastName))
                return Ok(new { success = false, message = "Last Name is required" });

            //check if the email is already taken
            var user = await _userManager.FindByEmailAsync(userDto.Email!);
            if (user != null)
                return Ok(new { success = false, message = "Email is already taken" });

            //get current user id
            var currentUserId = _userManager.GetUserId(User);

            var monthsUntilPasswordExpires =
                Convert.ToInt32(_configuration["SystemVariables:MonthsUntilPasswordExpires"]);

            var currentDateTimeOffset = DateTimeOffset.UtcNow;

            //create new user
            var newUser = new ApplicationUser
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                UserName = userDto.Email,
                NormalizedEmail = userDto.Email.ToUpper(),
                NormalizedUserName = userDto.Email.ToUpper(),
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                CanActionWkfTasks = (bool)userDto.CanActionWkfTask,
                IsEmployee = (bool)userDto.IsEmployee,
                SupervisorUserId = userDto.SupervisorUserId,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                AccountCreatedDate = currentDateTimeOffset,
                LastSuccessfulLoginDate = null,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTimeOffset,
                PasswordExpiryDate = currentDateTimeOffset.AddMonths(monthsUntilPasswordExpires),
                ChangePasswordOnNextLogin = false,
                RecentActivity = Status.AccountCreated.ToString()
            };

            //generate a random password with a capital letter
            var randomPassword =
                Guid.NewGuid()
                    .ToString()[..16]
                    .Insert(4, new string((char)new Random().Next(65, 90), 1));

            //create user
            var result = await _userManager.CreateAsync(newUser, randomPassword);

            if (!result.Succeeded)
                return Ok(new { success = false, message = "An error occurred while creating the user" });

            //create user detail
            await _userDetailService.CreateAsync(new UserDetail
            {
                UserId = newUser.Id,
                ProfileCompletionPercentage = 5,
                CreatedDate = currentDateTimeOffset,
                CreatedByUserId = currentUserId!
            });

            //save password history
            if (!string.IsNullOrEmpty(newUser.PasswordHash))
            {
                var passwordHistories = await _passwordHistoryService.GetPasswordHistories(newUser.Id);
                foreach (var passwordHistory in passwordHistories)
                {
                    passwordHistory.PasswordExpiryDate = currentDateTimeOffset;
                    passwordHistory.Status = Status.Expired.ToString();
                    passwordHistory.ModifiedByUserId = currentUserId;
                    passwordHistory.ModifiedDate = currentDateTimeOffset;
                    await _passwordHistoryService.UpdateAsync(passwordHistory);
                }

                //create new password history
                await _passwordHistoryService.CreateAsync(new PasswordHistory
                {
                    UserId = newUser.Id,
                    PasswordHash = newUser.PasswordHash,
                    PasswordCreatedDate = currentDateTimeOffset,
                    PasswordExpiryDate = (DateTimeOffset)newUser.PasswordExpiryDate,
                    CreatedByUserId = currentUserId!,
                    CreatedDate = currentDateTimeOffset
                });
            }

            //assign user role 'CLIENT' to the user
            var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
            var clientRole = await _roleManager.FindByNameAsync(clientRoleName);
            if (clientRole != null)
                await _userRoleService.CreateAsync(new ApplicationUserRole
                {
                    RoleId = clientRole.Id,
                    UserId = newUser.Id,
                    CreatedByUserId = currentUserId!,
                    CreatedDate = currentDateTimeOffset,
                    StartDate = currentDateTimeOffset,
                    EndDate = DateTimeOffset.Now.AddYears(3)
                });

            //send email to user to set new password
            var code = await _userManager.GeneratePasswordResetTokenAsync(newUser);
            var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            var body = _emailTemplate.RegisterAndResetPassword(
                newUser.FirstName,
                _util.HideEmailAddress(newUser.Email),
                HtmlEncoder.Default.Encode(callbackUrl!),
                Convert.ToInt32(expireTokenInHours)
            );

            await _emailSender.SendEmailAsync(
                newUser.Email,
                "Account Created",
                body,
                currentUserId
            );

            return Ok(new
            {
                success = true,
                message = "User created successfully. An email has been sent to the user's email address (" +
                          newUser.Email + ") to set a new password"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //send-password-reset-email
    [HttpPost("send-password-reset-email")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> SendPasswordResetEmail()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

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
                _util.HideEmailAddress(user.Email),
                HtmlEncoder.Default.Encode(callbackUrl!),
                Convert.ToInt32(expireTokenInHours)
            );

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset Password",
                body,
                _userManager.GetUserId(User)
            );

            return Ok(new
            {
                success = true,
                message = "Password reset email has been sent to the user"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("schedule-account-deletion")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> ScheduleAccountDeletion()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (user is { IsScheduledForDeletion: true, AccountDeletionScheduledDate: not null })
                return Ok(new
                {
                    success = false,
                    message = "User account has already been scheduled for deletion on " +
                              user.AccountDeletionScheduledDate?.ToString("dd/MM/yyyy")
                });

            var scheduledInDaysForAccountDeletion =
                Convert.ToDouble(_configuration["SystemVariables:ScheduledInDaysForAccountDeletion"]);

            var result = await _userManager.ScheduleAccountDeletionAsync(user, _userManager.GetUserId(User),
                scheduledInDaysForAccountDeletion);

            return result.Succeeded
                ? Ok(new
                {
                    success = true,
                    message = "User account has been successfully scheduled for to be deleted in " +
                              scheduledInDaysForAccountDeletion + " days on " +
                              user.AccountDeletionScheduledDate?.ToString("dd/MM/yyyy")
                })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("reverse-account-deletion")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> ReverseAccountDeletion()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (user is { IsScheduledForDeletion: false, AccountDeletionScheduledDate: null })
                return Ok(new { success = false, message = "User account has not been scheduled for deletion" });

            var result = await _userManager.ReverseAccountDeletionAsync(user, _userManager.GetUserId(User));
            return result.Succeeded
                ? Ok(new { success = true, message = "User account deletion has been successfully reversed" })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //LockAccount
    [HttpPost("lock-account")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> LockAccount()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (user is { IsScheduledForDeletion: true, AccountDeletionScheduledDate: not null })
                return Ok(new
                {
                    success = false,
                    message = "Cannot lock user account. User account has been scheduled for deletion on " +
                              user.AccountDeletionScheduledDate?.ToString("dd/MM/yyyy")
                });

            var result = await _userManager.LockAccountAsync(user, _userManager.GetUserId(User));
            return result.Succeeded
                ? Ok(new { success = true, message = "User account has been successfully locked" })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("unlock-account")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> UnlockAccount()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (user is { IsScheduledForDeletion: true, AccountDeletionScheduledDate: not null })
                return Ok(new
                {
                    success = false,
                    message = "Cannot unlock user account. User account has been scheduled for deletion on " +
                              user.AccountDeletionScheduledDate?.ToString("dd/MM/yyyy")
                });

            var result = await _userManager.UnlockAccountAsync(user, _userManager.GetUserId(User));
            return result.Succeeded
                ? Ok(new { success = true, message = "User account has been successfully unlocked" })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //disable-2fa
    [HttpPost("disable-2fa")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            return result.Succeeded
                ? Ok(new { success = true, message = "Two Factor Authentication has been successfully disabled" })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-user-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetUserData()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var profilePictureAttachmentPath = "";
            var userDetail = await _userDetailService.FindByUserIdAsync(user.Id);

            if (userDetail != null && !string.IsNullOrEmpty(userDetail.ProfilePictureAttachmentId))
            {
                var profilePictureAttachment =
                    await _systemAttachmentService.FindByIdAsync(userDetail.ProfilePictureAttachmentId);
                if (profilePictureAttachment != null)
                    profilePictureAttachmentPath = profilePictureAttachment.FilePath;
            }

            var supervisor = new ApplicationUser();
            if (!string.IsNullOrEmpty(user.SupervisorUserId))
                supervisor = await _userManager.FindByIdAsync(user.SupervisorUserId);

            supervisor ??= new ApplicationUser();

            //cast user to user dto
            var userDto = new UserDto
            {
                Id = _cypherService.Encrypt(user.Id),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd?.ToLocalTime(),
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                AccountCreatedDate = user.AccountCreatedDate.ToLocalTime(),
                LastSuccessfulLoginDate = user.LastSuccessfulLoginDate?.ToLocalTime(),
                LastSuccessfulPasswordChangeDate = user.LastSuccessfulPasswordChangeDate?.ToLocalTime(),
                AccountDeletionScheduledDate = user.AccountDeletionScheduledDate?.ToLocalTime(),
                IsScheduledForDeletion = user.IsScheduledForDeletion,
                IsEmployee = user.IsEmployee,
                CanActionWkfTask = user.CanActionWkfTasks,
                Status = user.RecentActivity,
                ProfilePictureAttachmentPath = profilePictureAttachmentPath,
                Supervisor = new UserDto
                {
                    Id = _cypherService.Encrypt(supervisor.Id),
                    FullName = supervisor.FullName,
                    Email = supervisor.Email,
                    FirstName = supervisor.FirstName,
                    LastName = supervisor.LastName
                }
            };

            return Ok(new { success = true, user = userDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("change-profile-picture")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> ChangeProfilePicture()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            var file = Request.Form.Files["file"];

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (file == null)
                return Ok(new { success = false, message = "File is required" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var isValid = _systemAttachmentManager.IsValid(file);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dto = await _systemAttachmentManager.UploadFile(file,
                AttachmentDirectory.ProfilePictures.GetDirectoryName(),
                true);

            if (dto == null)
                return Ok(new { success = false, message = "Unable to upload file. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;
            var systemAttachment = new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _systemAttachmentService.CreateAsync(systemAttachment);

            var userDetail = await _userDetailService.FindByUserIdAsync(userId);
            if (userDetail == null)
                return Ok(new { success = false, message = "User details not found" });

            //delete old profile picture from user details
            if (!string.IsNullOrEmpty(userDetail.ProfilePictureAttachmentId))
            {
                var oldProfilePictureAttachment =
                    await _systemAttachmentService.FindByIdAsync(userDetail.ProfilePictureAttachmentId);
                if (oldProfilePictureAttachment != null)
                    await _systemAttachmentService.DeleteAsync(oldProfilePictureAttachment);
            }

            userDetail.ProfilePictureAttachmentId = systemAttachment.Id;
            userDetail.ModifiedByUserId = currentUserId;
            userDetail.ModifiedDate = currentDateTime;
            await _userDetailService.UpdateAsync(userDetail);

            return Ok(new
            {
                success = true, message = "Profile picture changed successfully", filePath = systemAttachment.FilePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-user-details")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetUserDetails()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var userDetail = await _userDetailService.FindByUserIdAsync(user.Id);
            if (userDetail == null)
                return Ok(new { success = false, message = "User details not found" });

            var userDetailDto = new UserDetailDto
            {
                Id = _cypherService.Encrypt(userDetail.Id),
                UserId = _cypherService.Encrypt(userDetail.UserId),
                Gender = userDetail.Gender,
                DateOfBirth = userDetail.DateOfBirth,
                IdentityType = userDetail.IdentityType,
                IdentityNumber = userDetail.IdentityNumber,
                IdentityAttachmentId = _cypherService.Encrypt(userDetail.IdentityAttachmentId),
                CountryCode = _util.GetCountryCodeByCountryName(userDetail.Country),
                City = userDetail.City,
                User = new UserDto
                {
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberCountryCode = _util.GetCountryCodeByPhoneNumber(user.PhoneNumber),
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    IsEmployee = userDetail.User.IsEmployee
                },
                AlternativePhoneNumber = userDetail.AlternativePhoneNumber,
                AlternativePhoneNumberCountryCode =
                    _util.GetCountryCodeByPhoneNumber(userDetail.AlternativePhoneNumber),
                AlternativePhoneNumberConfirmed = userDetail.AlternativePhoneNumberConfirmed,
                AlternativeEmailAddress = userDetail.AlternativeEmailAddress,
                AlternativeEmailAddressConfirmed = userDetail.AlternativeEmailAddressConfirmed,
                PhysicalAddress = userDetail.PhysicalAddress,
                TermsOfUseAccepted = userDetail.TermsOfUseAccepted,
                PrivacyPolicyAccepted = userDetail.PrivacyPolicyAccepted,
                ProofOfResidencyAttachmentId = _cypherService.Encrypt(userDetail.ProofOfResidencyAttachmentId),
                NextOfKinFirstName = userDetail.NextOfKinFirstName,
                NextOfKinLastName = userDetail.NextOfKinLastName,
                NextOfKinIdentityType = userDetail.NextOfKinIdentityType,
                NextOfKinIdentityNumber = userDetail.NextOfKinIdentityNumber,
                NextOfKinIdentityAttachmentId = _cypherService.Encrypt(userDetail.NextOfKinIdentityAttachmentId),
                NextOfKinPhysicalAddress = userDetail.NextOfKinPhysicalAddress,
                NextOfKinGender = userDetail.NextOfKinGender,
                NextOfKinPhoneNumber = userDetail.NextOfKinPhoneNumber,
                NextOfKinPhoneNumberCountryCode = _util.GetCountryCodeByPhoneNumber(userDetail.NextOfKinPhoneNumber),
                NextOfKinPhoneNumberConfirmed = userDetail.NextOfKinPhoneNumberConfirmed,
                NextOfKinCountryCode = _util.GetCountryCodeByCountryName(userDetail.NextOfKinCountry),
                NextOfKinCity = userDetail.NextOfKinCity,
                NextOfKinEmailAddress = userDetail.NextOfKinEmailAddress,
                NextOfKinEmailAddressConfirmed = userDetail.NextOfKinEmailAddressConfirmed,
                NextOfKinProofOfResidencyAttachmentId =
                    _cypherService.Encrypt(userDetail.NextOfKinProofOfResidencyAttachmentId),
                ProfileCompletionPercentage = userDetail.ProfileCompletionPercentage
            };

            return Ok(new { success = true, userDetail = userDetailDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit-user-details")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> EditUserDetails()
    {
        try
        {
            var dto = new UserDetailDto
            {
                Id = Request.Form["userDetailId"].FirstOrDefault(),
                UserId = Request.Form["userId"].FirstOrDefault(),
                Gender = Request.Form["gender"].FirstOrDefault(),
                DateOfBirthString = Request.Form["dateOfBirth"].FirstOrDefault(),
                IdentityType = Request.Form["identityType"].FirstOrDefault(),
                IdentityNumber = Request.Form["identityNumber"].FirstOrDefault(),
                CountryCode = Request.Form["countryCode"].FirstOrDefault(),
                City = Request.Form["city"].FirstOrDefault(),
                AlternativePhoneNumber = Request.Form["alternativePhoneNumber"].FirstOrDefault(),
                User = new UserDto { PhoneNumber = Request.Form["phoneNumber"].FirstOrDefault() },
                AlternativeEmailAddress = Request.Form["alternativeEmailAddress"].FirstOrDefault(),
                PhysicalAddress = Request.Form["physicalAddress"].FirstOrDefault(),
                TermsOfUseAcceptedString = Request.Form["termsOfUseAccepted"].FirstOrDefault(),
                PrivacyPolicyAcceptedString = Request.Form["privacyPolicyAccepted"].FirstOrDefault(),
                NextOfKinFirstName = Request.Form["nextOfKinFirstName"].FirstOrDefault(),
                NextOfKinLastName = Request.Form["nextOfKinLastName"].FirstOrDefault(),
                NextOfKinIdentityType = Request.Form["nextOfKinIdentityType"].FirstOrDefault(),
                NextOfKinIdentityNumber = Request.Form["nextOfKinIdentityNumber"].FirstOrDefault(),
                NextOfKinPhysicalAddress = Request.Form["nextOfKinPhysicalAddress"].FirstOrDefault(),
                NextOfKinGender = Request.Form["nextOfKinGender"].FirstOrDefault(),
                NextOfKinPhoneNumber = Request.Form["nextOfKinPhoneNumber"].FirstOrDefault(),
                NextOfKinCountryCode = Request.Form["nextOfKinCountryCode"].FirstOrDefault(),
                NextOfKinCity = Request.Form["nextOfKinCity"].FirstOrDefault(),
                NextOfKinEmailAddress = Request.Form["nextOfKinEmailAddress"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(dto.Id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            dto.Id = _cypherService.Decrypt(dto.Id);
            var userDetail = await _userDetailService.FindByIdAsync(dto.Id);

            if (userDetail == null)
                return Ok(new { success = false, message = "User details not found" });

            if (string.IsNullOrEmpty(dto.UserId))
                return Ok(new { success = false, message = "User not found" });

            dto.UserId = _cypherService.Decrypt(dto.UserId);
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            if (!string.IsNullOrEmpty(dto.Gender))
            {
                userDetail.Gender = dto.Gender;
            }

            if (!string.IsNullOrEmpty(dto.DateOfBirthString))
            {
                var dob = _util.ConvertStringToDateTimeOffset(dto.DateOfBirthString);
                //if age is less than 16 years
                if (dob > DateTimeOffset.UtcNow.AddYears(-16))
                    return Ok(new { success = false, message = "Must be at least 16 years old." });

                userDetail.DateOfBirth = dob;
            }

            if (!string.IsNullOrEmpty(dto.IdentityNumber) && !string.IsNullOrEmpty(dto.IdentityType))
            {
                //if character length of identity number is less than 5
                if (dto.IdentityNumber.Length < 5)
                    return Ok(new { success = false, message = "Identity Number is invalid" });

                //check if user's identity number is equal to next of kin's identity number
                if (dto.IdentityType == dto.NextOfKinIdentityType && dto.IdentityNumber == dto.NextOfKinIdentityNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Identity Number cannot be the same as Next of Kin's Identity Number"
                    });

                //if the identity number already used
                var userWithIdentityNumber =
                    await _userDetailService.FindAllByIdentityTypeAndIdentityNumberExceptByUserDetailIdAsync(
                        dto.IdentityType, dto.IdentityNumber, userDetail.Id);
                if (userWithIdentityNumber.Any())
                    return Ok(new { success = false, message = "Identity Number is already taken" });

                userDetail.IdentityType = dto.IdentityType;
                userDetail.IdentityNumber = dto.IdentityNumber;
            }
            else if (!string.IsNullOrEmpty(dto.IdentityNumber) && string.IsNullOrEmpty(dto.IdentityType))
            {
                return Ok(new { success = false, message = "Identity Type is required" });
            }
            else if (string.IsNullOrEmpty(dto.IdentityNumber) && !string.IsNullOrEmpty(dto.IdentityType))
            {
                return Ok(new { success = false, message = "Identity Number is required" });
            }

            if (!string.IsNullOrEmpty(dto.CountryCode))
            {
                userDetail.Country = _util.GetCountryNameByCountryCode(dto.CountryCode);
            }

            if (!string.IsNullOrEmpty(dto.City))
            {
                userDetail.City = dto.City;
            }

            if (!string.IsNullOrEmpty(dto.User.PhoneNumber))
            {
                var result = _util.ValidatePhoneNumber(dto.User.PhoneNumber, "Phone Number");

                if (!result.IsValid)
                    return Ok(new { success = false, message = result.Response });

                //if the phone number is equal to alternative phone number or next of kin's phone number
                if (dto.User.PhoneNumber == dto.AlternativePhoneNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Phone Number cannot be the same as Alternative Phone Number"
                    });

                if (dto.User.PhoneNumber == dto.NextOfKinPhoneNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Phone Number cannot be the same as Next of Kin's Phone Number"
                    });

                //if the phone number already used
                var userWithPhoneNumber1 =
                    await _userService.FindAllByPhoneNumberExceptByUserIdAsync(dto.User.PhoneNumber,
                        user.Id);
                if (userWithPhoneNumber1.Any())
                    return Ok(new { success = false, message = "Phone Number is already taken" });

                var userWithPhoneNumber2 =
                    await _userDetailService.FindAllByAlternativePhoneNumberExceptByUserDetailIdAsync(
                        dto.User.PhoneNumber, userDetail.Id);
                if (userWithPhoneNumber2.Any())
                    return Ok(new { success = false, message = "Phone Number is already taken" });

                var userWithPhoneNumber3 =
                    await _userDetailService.FindAllByNextOfKinPhoneNumberExceptByUserDetailIdAsync(
                        dto.User.PhoneNumber, userDetail.Id);
                if (userWithPhoneNumber3.Any())
                    return Ok(new { success = false, message = "Phone Number is already taken" });

                user.PhoneNumber = dto.User.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(dto.AlternativePhoneNumber))
            {
                var result = _util.ValidatePhoneNumber(dto.AlternativePhoneNumber, "Alternative Phone Number");

                if (!result.IsValid)
                    return Ok(new { success = false, message = result.Response });

                //if the phone number is equal to alternative phone number or next of kin's phone number
                if (dto.AlternativePhoneNumber == dto.User.PhoneNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Alternative Phone Number cannot be the same as the user's Phone Number"
                    });

                if (dto.AlternativePhoneNumber == dto.NextOfKinPhoneNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Alternative Phone Number cannot be the same as Next of Kin's Phone Number"
                    });

                //if the alternative phone number already used
                var userWithAlternativePhoneNumber1 =
                    await _userDetailService.FindAllByAlternativePhoneNumberExceptByUserDetailIdAsync(
                        dto.AlternativePhoneNumber, userDetail.Id);
                if (userWithAlternativePhoneNumber1.Any())
                    return Ok(new { success = false, message = "Alternative Phone Number is already taken" });

                var userWithAlternativePhoneNumber2 =
                    await _userService.FindAllByPhoneNumberExceptByUserIdAsync(dto.AlternativePhoneNumber, user.Id);
                if (userWithAlternativePhoneNumber2.Any())
                    return Ok(new { success = false, message = "Alternative Phone Number is already taken" });

                var userWithAlternativePhoneNumber3 =
                    await _userDetailService.FindAllByNextOfKinPhoneNumberExceptByUserDetailIdAsync(
                        dto.AlternativePhoneNumber, userDetail.Id);
                if (userWithAlternativePhoneNumber3.Any())
                    return Ok(new { success = false, message = "Alternative Phone Number is already taken" });

                userDetail.AlternativePhoneNumber = dto.AlternativePhoneNumber;
            }

            if (!string.IsNullOrEmpty(dto.AlternativeEmailAddress))
            {
                if (!_util.IsValidEmail(dto.AlternativeEmailAddress))
                    return Ok(new { success = false, message = "Alternative Email Address is invalid" });

                //if the alternative email is equal to user's email or next of kin's email
                if (dto.AlternativeEmailAddress == user.Email)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Alternative Email Address cannot be the same as User's Email Address"
                    });

                if (dto.AlternativeEmailAddress == dto.NextOfKinEmailAddress)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Alternative Email Address cannot be the same as Next of Kin's Email Address"
                    });

                //if the alternative email already used
                var userWithAlternativeEmail1 =
                    await _userDetailService.FindAllByAlternativeEmailAddressExceptByUserDetailIdAsync(
                        dto.AlternativeEmailAddress, userDetail.Id);
                if (userWithAlternativeEmail1.Any())
                    return Ok(new { success = false, message = "Alternative Email Address is already taken" });

                var userWithAlternativeEmail2 =
                    await _userService.FindAllByEmailAddressExceptByUserIdAsync(dto.AlternativeEmailAddress, user.Id);
                if (userWithAlternativeEmail2.Any())
                    return Ok(new { success = false, message = "Alternative Email Address is already taken" });

                var userWithAlternativeEmail3 =
                    await _userDetailService.FindAllByNextOfKinEmailAddressExceptByUserDetailIdAsync(
                        dto.AlternativeEmailAddress, userDetail.Id);
                if (userWithAlternativeEmail3.Any())
                    return Ok(new { success = false, message = "Alternative Email Address is already taken" });

                userDetail.AlternativeEmailAddress = dto.AlternativeEmailAddress;
            }

            if (!string.IsNullOrEmpty(dto.PhysicalAddress))
            {
                userDetail.PhysicalAddress = dto.PhysicalAddress;
            }

            if (!string.IsNullOrEmpty(dto.TermsOfUseAcceptedString))
            {
                if (dto.TermsOfUseAcceptedString == "on")
                    dto.TermsOfUseAcceptedString = "true";
                userDetail.TermsOfUseAccepted = bool.Parse(dto.TermsOfUseAcceptedString);
            }

            if (!string.IsNullOrEmpty(dto.PrivacyPolicyAcceptedString))
            {
                if (dto.PrivacyPolicyAcceptedString == "on")
                    dto.PrivacyPolicyAcceptedString = "true";
                userDetail.PrivacyPolicyAccepted = bool.Parse(dto.PrivacyPolicyAcceptedString);
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinFirstName))
            {
                userDetail.NextOfKinFirstName = dto.NextOfKinFirstName;
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinLastName))
            {
                userDetail.NextOfKinLastName = dto.NextOfKinLastName;
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinIdentityType) && !string.IsNullOrEmpty(dto.NextOfKinIdentityNumber))
            {
                //if character length of identity number is less than 5
                if (dto.NextOfKinIdentityNumber.Length < 5)
                    return Ok(new { success = false, message = "Next of Kin's Identity Number is invalid" });

                //check if the next of kin identity number is the same as user's identity number
                if (dto.NextOfKinIdentityType == dto.IdentityType && dto.NextOfKinIdentityNumber == dto.IdentityNumber)
                    return Ok(new
                    {
                        success = false,
                        message = "Next of Kin's Identity Number cannot be the same as User's Identity Number"
                    });

                //save
                userDetail.NextOfKinIdentityType = dto.NextOfKinIdentityType;
                userDetail.NextOfKinIdentityNumber = dto.NextOfKinIdentityNumber;
            }
            else if (!string.IsNullOrEmpty(dto.NextOfKinIdentityNumber) &&
                     string.IsNullOrEmpty(dto.NextOfKinIdentityType))
            {
                return Ok(new { success = false, message = "Next of Kin's Identity Type is required" });
            }
            else if (string.IsNullOrEmpty(dto.NextOfKinIdentityNumber) &&
                     !string.IsNullOrEmpty(dto.NextOfKinIdentityType))
            {
                return Ok(new { success = false, message = "Next of Kin's Identity Number is required" });
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinPhysicalAddress))
            {
                userDetail.NextOfKinPhysicalAddress = dto.NextOfKinPhysicalAddress;
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinGender))
            {
                userDetail.NextOfKinGender = dto.NextOfKinGender;
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinPhoneNumber))
            {
                var result = _util.ValidatePhoneNumber(dto.NextOfKinPhoneNumber, "Next of Kin's Phone Number");

                if (!result.IsValid)
                    return Ok(new { success = false, message = result.Response });

                //if the next of kin phone number is equal to user's phone number or alternative phone number
                if (dto.NextOfKinPhoneNumber == dto.User.PhoneNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Next of Kin's Phone Number cannot be the same as User's Phone Number"
                    });

                if (dto.NextOfKinPhoneNumber == dto.AlternativePhoneNumber)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Next of Kin's Phone Number cannot be the same as Alternative Phone Number"
                    });


                //if the next of kin phone number already used
                var userWithNextOfKinPhoneNumber1 =
                    await _userDetailService.FindAllByNextOfKinPhoneNumberExceptByUserDetailIdAsync(
                        dto.NextOfKinPhoneNumber, userDetail.Id);
                if (userWithNextOfKinPhoneNumber1.Any())
                    return Ok(new { success = false, message = "Next of Kin's Phone Number is already taken" });

                var userWithNextOfKinPhoneNumber2 =
                    await _userService.FindAllByPhoneNumberExceptByUserIdAsync(dto.NextOfKinPhoneNumber, user.Id);
                if (userWithNextOfKinPhoneNumber2.Any())
                    return Ok(new { success = false, message = "Next of Kin's Phone Number is already taken" });

                var userWithNextOfKinPhoneNumber3 =
                    await _userDetailService.FindAllByAlternativePhoneNumberExceptByUserDetailIdAsync(
                        dto.NextOfKinPhoneNumber, userDetail.Id);
                if (userWithNextOfKinPhoneNumber3.Any())
                    return Ok(new { success = false, message = "Next of Kin's Phone Number is already taken" });

                userDetail.NextOfKinPhoneNumber = dto.NextOfKinPhoneNumber;
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinCountryCode))
            {
                userDetail.NextOfKinCountry = _util.GetCountryNameByCountryCode(dto.NextOfKinCountryCode);
                /*userDetail.NextOfKinCountry = Enum.GetValues<Country>()
                    .FirstOrDefault(c => c.GetCountryCode() == dto.NextOfKinCountryCode)
                    .GetCountryName();*/
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinCity))
            {
                userDetail.NextOfKinCity = dto.NextOfKinCity;
            }

            if (!string.IsNullOrEmpty(dto.NextOfKinEmailAddress))
            {
                if (!_util.IsValidEmail(dto.NextOfKinEmailAddress))
                    return Ok(new { success = false, message = "Next of Kin's Email Address is invalid" });

                //if the next of kin email is equal to user's email or alternative email
                if (dto.NextOfKinEmailAddress == user.Email)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Next of Kin's Email Address cannot be the same as User's Email Address"
                    });

                if (dto.NextOfKinEmailAddress == dto.AlternativeEmailAddress)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "Next of Kin's Email Address cannot be the same as Alternative Email Address"
                    });

                //check if the next of kin email is already used
                var userWithNextOfKinEmail1 =
                    await _userDetailService.FindAllByNextOfKinEmailAddressExceptByUserDetailIdAsync(
                        dto.NextOfKinEmailAddress, userDetail.Id);
                if (userWithNextOfKinEmail1.Any())
                    return Ok(new { success = false, message = "Next of Kin's Email Address is already taken" });

                var userWithNextOfKinEmail2 =
                    await _userService.FindAllByEmailAddressExceptByUserIdAsync(dto.NextOfKinEmailAddress, user.Id);
                if (userWithNextOfKinEmail2.Any())
                    return Ok(new { success = false, message = "Next of Kin's Email Address is already taken" });

                var userWithNextOfKinEmail3 =
                    await _userDetailService.FindAllByAlternativeEmailAddressExceptByUserDetailIdAsync(
                        dto.NextOfKinEmailAddress, userDetail.Id);
                if (userWithNextOfKinEmail3.Any())
                    return Ok(new { success = false, message = "Next of Kin's Email Address is already taken" });

                userDetail.NextOfKinEmailAddress = dto.NextOfKinEmailAddress;
            }

            userDetail.ProfileCompletionPercentage =
                _userDetailService.CalculateProfileCompletionPercentage(userDetail);
            await _userDetailService.UpdateAsync(userDetail);
            await _userManager.UpdateAsync(user);

            return Ok(new { success = true, message = "User details updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("upload-identity-attachment")]
    public async Task<IActionResult> UploadFormFile()
    {
        try
        {
            var file = Request.Form.Files["file"];

            if (file == null)
                return Ok(new { success = false, message = "File is required" });

            var userDetailId = Request.Form["userDetailId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);

            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var isValid = _systemAttachmentManager.IsValid(file);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dto = await _systemAttachmentManager.UploadFile(file, AttachmentDirectory.Common.GetDirectoryName());

            if (dto == null)
                return Ok(new { success = false, message = "Unable to upload file. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;
            var systemAttachment = new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _systemAttachmentService.CreateAsync(systemAttachment);


            userDetail.IdentityAttachmentId = systemAttachment.Id;
            userDetail.ModifiedByUserId = currentUserId;
            userDetail.ModifiedDate = currentDateTime;
            await _userDetailService.UpdateAsync(userDetail);

            var encryptedAttachmentId = _cypherService.Encrypt(systemAttachment.Id);

            return Ok(new
                { success = true, message = "Attachment uploaded successfully", attachmentId = encryptedAttachmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //upload-proof-of-residency-attachment
    [HttpPost("upload-proof-of-residency-attachment")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> UploadProofOfResidencyAttachment()
    {
        try
        {
            var file = Request.Form.Files["file"];

            if (file == null)
                return Ok(new { success = false, message = "File is required" });

            var userDetailId = Request.Form["userDetailId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);

            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var isValid = _systemAttachmentManager.IsValid(file);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dto = await _systemAttachmentManager.UploadFile(file, AttachmentDirectory.Common.GetDirectoryName());

            if (dto == null)
                return Ok(new { success = false, message = "Unable to upload file. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;
            var systemAttachment = new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _systemAttachmentService.CreateAsync(systemAttachment);


            userDetail.ProofOfResidencyAttachmentId = systemAttachment.Id;
            userDetail.ModifiedByUserId = currentUserId;
            userDetail.ModifiedDate = currentDateTime;
            await _userDetailService.UpdateAsync(userDetail);

            var encryptedAttachmentId = _cypherService.Encrypt(systemAttachment.Id);

            return Ok(new
                { success = true, message = "Attachment uploaded successfully", attachmentId = encryptedAttachmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //upload-next-of-kin-proof-of-residency-attachment
    [HttpPost("upload-next-of-kin-proof-of-residency-attachment")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> UploadNextOfKinProofOfResidencyAttachment()
    {
        try
        {
            var file = Request.Form.Files["file"];

            if (file == null)
                return Ok(new { success = false, message = "File is required" });

            var userDetailId = Request.Form["userDetailId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);

            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var isValid = _systemAttachmentManager.IsValid(file);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dto = await _systemAttachmentManager.UploadFile(file, AttachmentDirectory.Common.GetDirectoryName());

            if (dto == null)
                return Ok(new { success = false, message = "Unable to upload file. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;
            var systemAttachment = new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _systemAttachmentService.CreateAsync(systemAttachment);

            userDetail.NextOfKinProofOfResidencyAttachmentId = systemAttachment.Id;
            userDetail.ModifiedByUserId = currentUserId;
            userDetail.ModifiedDate = currentDateTime;
            await _userDetailService.UpdateAsync(userDetail);

            var encryptedAttachmentId = _cypherService.Encrypt(systemAttachment.Id);

            return Ok(new
                { success = true, message = "Attachment uploaded successfully", attachmentId = encryptedAttachmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //upload-next-of-kin-identity-attachment
    [HttpPost("upload-next-of-kin-identity-attachment")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> UploadNextOfKinIdentityAttachment()
    {
        try
        {
            var file = Request.Form.Files["file"];

            if (file == null)
                return Ok(new { success = false, message = "File is required" });

            var userDetailId = Request.Form["userDetailId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);

            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var isValid = _systemAttachmentManager.IsValid(file);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dto = await _systemAttachmentManager.UploadFile(file, AttachmentDirectory.Common.GetDirectoryName());

            if (dto == null)
                return Ok(new { success = false, message = "Unable to upload file. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;
            var systemAttachment = new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _systemAttachmentService.CreateAsync(systemAttachment);

            userDetail.NextOfKinIdentityAttachmentId = systemAttachment.Id;
            userDetail.ModifiedByUserId = currentUserId;
            userDetail.ModifiedDate = currentDateTime;
            await _userDetailService.UpdateAsync(userDetail);

            var encryptedAttachmentId = _cypherService.Encrypt(systemAttachment.Id);

            return Ok(new
                { success = true, message = "Attachment uploaded successfully", attachmentId = encryptedAttachmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }


    [HttpPost("edit-user")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> EditUser()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault();
            var firstName = Request.Form["firstName"].FirstOrDefault();
            var lastName = Request.Form["lastName"].FirstOrDefault();
            var email = Request.Form["email"].FirstOrDefault();
            var isEmployeeString = Request.Form["isEmployee"].FirstOrDefault();
            var canActionWkfTaskString = Request.Form["canActionWkfTask"].FirstOrDefault();
            var supervisorUserId = Request.Form["supervisorUserId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });
            if (string.IsNullOrEmpty(firstName))
                return Ok(new { success = false, message = "First Name is required" });
            if (string.IsNullOrEmpty(lastName))
                return Ok(new { success = false, message = "Last Name is required" });
            if (string.IsNullOrEmpty(email))
                return Ok(new { success = false, message = "Email is required" });

            bool isEmployee;
            if (string.IsNullOrEmpty(isEmployeeString))
                isEmployee = false;
            else if (isEmployeeString is "on" or "true")
                isEmployee = true;
            else
                isEmployee = false;

            bool canActionWkfTask;
            if (string.IsNullOrEmpty(canActionWkfTaskString))
                canActionWkfTask = false;
            else if (canActionWkfTaskString is "on" or "true")
                canActionWkfTask = true;
            else
                canActionWkfTask = false;

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            //validate email
            if (!_util.IsValidEmail(email))
                return Ok(new { success = false, message = "Email is invalid" });


            // if the user is NOT an employee, remove all roles and add client role
            var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
            if (!isEmployee)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                    await _userManager.RemoveFromRolesAsync(user, roles);

                //add the client role
                var clientRole = await _roleManager.FindByNameAsync(clientRoleName);
                if (clientRole != null)
                    await _userRoleService.CreateAsync(new ApplicationUserRole
                    {
                        RoleId = clientRole.Id,
                        UserId = user.Id,
                        CreatedByUserId = _configuration["SystemAdminAccount:UserId"] ?? string.Empty,
                        CreatedDate = DateTimeOffset.UtcNow,
                        StartDate = DateTimeOffset.UtcNow,
                        EndDate = DateTimeOffset.MaxValue
                    });
            }


            //get current user id
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(supervisorUserId))
                user.SupervisorUserId = null;
            else
            {
                supervisorUserId = _cypherService.Decrypt(supervisorUserId);

                //check if the supervisor user exists
                var supervisorUser = await _userManager.FindByIdAsync(supervisorUserId);
                if (supervisorUser == null)
                    return Ok(new { success = false, message = "Supervisor not found" });

                user.SupervisorUserId = supervisorUserId;
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.IsEmployee = isEmployee;
            user.CanActionWkfTasks = canActionWkfTask;
            user.RecentActivity = Status.AccountEdited.ToString();
            user.ModifiedByUserId = currentUserId;
            user.ModifiedDate = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            //if the email is changed, check if the new email is already taken
            if (email.Equals(user.Email)) return Ok(new { success = true, message = "User updated successfully" });

            //get userDetail
            var userDetail = await _userDetailService.FindByUserIdAsync(user.Id);
            if (userDetail == null)
                return Ok(new { success = false, message = "User details not found" });

            //check if the new email is already used as alternative email or next of kin email
            if (email.Equals(userDetail.AlternativeEmailAddress))
                return Ok(new { success = false, message = "Email is used as Alternative Email Address" });

            if (email.Equals(userDetail.NextOfKinEmailAddress))
                return Ok(new { success = false, message = "Email is used as Next of Kin's Email Address" });

            var userWithEmail = await _userManager.FindByEmailAsync(email);
            if (userWithEmail != null)
                return Ok(new { success = false, message = "Email is already taken" });

            //if email is changed, send email to user to verify new email
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, email);

            var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

            userId = _cypherService.Encrypt(userId);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId, email, code },
                protocol: Request.Scheme);

            var body = _emailTemplate.ConfirmEmail(
                user.FirstName,
                _util.HideEmailAddress(email),
                HtmlEncoder.Default.Encode(callbackUrl!),
                Convert.ToInt32(expireTokenInHours)
            );

            await _emailSender.SendEmailAsync(
                email,
                "Confirm Email",
                body,
                currentUserId
            );

            return Ok(new
            {
                success = true,
                message =
                    "User updated successfully. A verification email has been sent to the new email address: " + email +
                    "."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-user-password-history-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetUserPasswordHistoryDt()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var passwordHistories = await _passwordHistoryService.GetPasswordHistoryDtos(userId);

            //order by password created date
            passwordHistories = passwordHistories.OrderByDescending(ph => ph.PasswordCreatedDate).ToList();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form, _ => true,
                passwordHistoryDto => passwordHistoryDto,
                passwordHistories.Select((passwordHistory, index) => new PasswordHistoryDto
                {
                    Id = _cypherService.Encrypt(passwordHistory.Id!),
                    Counter = index + 1,
                    PasswordCreatedDate = passwordHistory.PasswordCreatedDate.ToLocalTime(),
                    PasswordExpiryDate = passwordHistory.PasswordExpiryDate.ToLocalTime(),
                    PasswordExpiryTimeLeft = _util.GetTimeLeftInWords(passwordHistory.PasswordExpiryDate, "Expired"),
                    Status = DateTimeOffset.Now.ToLocalTime() >
                             passwordHistory.PasswordExpiryDate.ToLocalTime()
                        ? Status.Expired.ToString()
                        : Status.Active.ToString()
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-external-logins-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetExternalLogins()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var externalLogins = await _userManager.GetLoginsAsync(user);

            //return datatable response
            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                _ => true,
                externalLogin => externalLogin,
                externalLogins.Select((externalLogin, index) => new ExternalLoginDto
                {
                    Counter = index + 1,
                    LoginProvider = externalLogin.LoginProvider,
                    ProviderDisplayName = externalLogin.ProviderDisplayName
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-user-2fa-status-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetUserTwoFactorAuthenticationStatus()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var recoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            return _datatableService.GetEntitiesForDatatable(Request.Form,
                _ => true,
                twoFactorAuthenticationDto => twoFactorAuthenticationDto,
                new List<TwoFactorAuthenticationDto>
                {
                    new()
                    {
                        Counter = 1,
                        Is2faEnabled = twoFactorEnabled,
                        RecoveryCodesLeft = recoveryCodesLeft
                    }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("verify-email-address")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> VerifyEmailAddress()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var email = user.Email;
            if (string.IsNullOrEmpty(email))
                return Ok(new { success = false, message = "Email is required" });

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];

            userId = _cypherService.Encrypt(userId);

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
                email,
                "Confirm Email",
                body,
                _userManager.GetUserId(User)
            );

            return Ok(new
                { success = true, message = "Verification link has been sent to this email address: " + email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //verify-next-of-kin-email-address
    [HttpPost("verify-next-of-kin-email-address")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> VerifyNextOfKinEmailAddress()
    {
        try
        {
            var userDetailId = Request.Form["userDetailId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);
            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var email = userDetail.NextOfKinEmailAddress;
            if (string.IsNullOrEmpty(email))
                return Ok(new { success = false, message = "Email is required" });

            if (string.IsNullOrEmpty(userDetail.NextOfKinFirstName))
                return Ok(new { success = false, message = "Next of Kin's First Name is required" });

            if (string.IsNullOrEmpty(userDetail.NextOfKinLastName))
                return Ok(new { success = false, message = "Next of Kin's Last Name is required" });

            if (string.IsNullOrEmpty(userDetail.User.FullName))
                return Ok(new { success = false, message = "Next of Kin's Phone Number is required" });

            if (string.IsNullOrEmpty(userDetail.User.PhoneNumber))
                return Ok(new { success = false, message = "User's Phone Number is required" });

            var token = _util.GenerateGuid();
            var encryptedToken = _cypherService.Encrypt(token);
            var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];
            var currentDateTime = DateTimeOffset.UtcNow;
            var userId = _cypherService.Encrypt(userDetail.User.Id);

            //save email verification token in the database
            await _emailVerificationService.CreateAsync(new EmailVerification
            {
                UserId = userDetail.User.Id,
                Email = email,
                Token = token,
                EmailType = EmailVerificationType.NextOfKin.ToString(),
                DateInitiated = currentDateTime,
                ExpiryDate = currentDateTime.AddHours(Convert.ToInt32(expireTokenInHours)),
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = currentDateTime
            });

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail2",
                pageHandler: null,
                values: new
                {
                    area = "Identity", userId, code = encryptedToken,
                    emailType = EmailVerificationType.NextOfKin.ToString()
                },
                protocol: Request.Scheme);

            var body = _emailTemplate.ConfirmNextOfKinEmail(
                userDetail.NextOfKinFirstName! + " " + userDetail.NextOfKinLastName!,
                userDetail.User.FullName,
                userDetail.User.PhoneNumber,
                HtmlEncoder.Default.Encode(callbackUrl!),
                Convert.ToInt32(expireTokenInHours)
            );

            await _emailSender.SendEmailAsync(
                email,
                "Confirm Email",
                body,
                _userManager.GetUserId(User)
            );

            return Ok(new
            {
                success = true,
                message = "A verification link has been sent to the Next of Kin's email address. Email: " + email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //verify-alternative-email-address
    [HttpPost("verify-alternative-email-address")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> VerifyAlternativeEmailAddress()
    {
        try
        {
            var userDetailId = Request.Form["userDetailId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);
            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var email = userDetail.AlternativeEmailAddress;
            if (string.IsNullOrEmpty(email))
                return Ok(new { success = false, message = "Email is required" });

            var token = _util.GenerateGuid();
            var encryptedToken = _cypherService.Encrypt(token);
            var expireTokenInHours = _configuration["SystemVariables:ExpireVerificationTokenInHours"];
            var currentDateTime = DateTimeOffset.UtcNow;
            var userId = _cypherService.Encrypt(userDetail.User.Id);

            //save email verification token in the database
            await _emailVerificationService.CreateAsync(new EmailVerification
            {
                UserId = userDetail.User.Id,
                Email = email,
                Token = token,
                EmailType = EmailVerificationType.Alternative.ToString(),
                DateInitiated = currentDateTime,
                ExpiryDate = currentDateTime.AddHours(Convert.ToInt32(expireTokenInHours)),
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = currentDateTime
            });

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail2",
                pageHandler: null,
                values: new
                {
                    area = "Identity", userId, code = encryptedToken,
                    emailType = EmailVerificationType.Alternative.ToString()
                },
                protocol: Request.Scheme);

            var body = _emailTemplate.ConfirmEmail(
                userDetail.User.FirstName,
                _util.HideEmailAddress(email),
                HtmlEncoder.Default.Encode(callbackUrl!),
                Convert.ToInt32(expireTokenInHours)
            );

            await _emailSender.SendEmailAsync(
                email,
                "Confirm Email",
                body,
                _userManager.GetUserId(User)
            );

            return Ok(new
                { success = true, message = "Verification link has been sent to this email address: " + email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //verify-phone-number
    [HttpPost("verify-phone-number")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> VerifyPhoneNumber()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var phoneNumber = user.PhoneNumber;
            if (string.IsNullOrEmpty(phoneNumber))
                return Ok(new { success = false, message = "Phone Number is required" });

            var currentUserId = _userManager.GetUserId(User);

            var otp = _otpService.GenerateOtp(user.Id);

            await _smsSender.SendSmsAsync(phoneNumber,
                $"You are about to verify your phone number. Please use the following {_configuration["Otp:Length"]}-digit OTP code: {otp}",
                currentUserId!);

            return Ok(new { success = true, message = "OTP sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //verify-next-of-kin-phone-number
    [HttpPost("verify-next-of-kin-phone-number")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> VerifyNextOfKinPhoneNumber()
    {
        try
        {
            var userDetailId = Request.Form["userDetailId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);
            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var phoneNumber = userDetail.NextOfKinPhoneNumber;
            if (string.IsNullOrEmpty(phoneNumber))
                return Ok(new { success = false, message = "Phone Number is required" });

            var currentUserId = _userManager.GetUserId(User);

            var otp = _otpService.GenerateOtp(userDetail.User.Id);

            await _smsSender.SendSmsAsync(phoneNumber,
                $"You are about to verify your phone number. Please use the following {_configuration["Otp:Length"]}-digit OTP code: {otp}",
                currentUserId!);

            return Ok(new { success = true, message = "OTP code sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //verify-alternative-phone-number
    [HttpPost("verify-alternative-phone-number")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> VerifyAlternativePhoneNumber()
    {
        try
        {
            var userDetailId = Request.Form["userDetailId"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userDetailId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userDetailId = _cypherService.Decrypt(userDetailId);
            var userDetail = await _userDetailService.FindByIdAsync(userDetailId);
            if (userDetail == null)
                return Ok(new { success = false, message = "User not found" });

            var phoneNumber = userDetail.AlternativePhoneNumber;
            if (string.IsNullOrEmpty(phoneNumber))
                return Ok(new { success = false, message = "Phone Number is required" });

            var currentUserId = _userManager.GetUserId(User);

            var otp = _otpService.GenerateOtp(userDetail.User.Id);

            await _smsSender.SendSmsAsync(phoneNumber,
                $"You are about to verify your phone number. Please use the following {_configuration["Otp:Length"]}-digit OTP code: {otp}",
                currentUserId!);

            return Ok(new { success = true, message = "OTP code sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("validate-otp")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> ValidateOtp()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var otp = Request.Form["otp"].FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(otp))
                return Ok(new { success = false, message = "OTP is required" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var isValid = _otpService.IsValidOtp(user.Id, otp);
            return Ok(!isValid
                ? new { success = false, message = "Invalid OTP" }
                : new { success = true, message = "OTP is valid" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-employees-select2
    [HttpPost("get-employees-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetEmployeesSelect2()
    {
        try
        {
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;

            //get all employees
            var employees = await _userService.FindAllEmployeesAsync();

            // Filter the employees based on the search term
            var employeesQuery = employees
                .Where(employee =>
                    employee.Email != null && (employee.Email.Contains(q) || employee.FirstName.Contains(q) ||
                                               employee.LastName.Contains(q))).ToList();

            var totalCount = employeesQuery.Count;

            var employeesList = employeesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(employee => new UserDto
                {
                    Id = _cypherService.Encrypt(employee.Id),
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email
                })
                .ToList();

            return Ok(new { results = employeesList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-employees-except-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetEmployeesExceptSelect2()
    {
        try
        {
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;

            //get all employees
            userId = _cypherService.Decrypt(userId);
            var employees = await _userService.FindAllEmployeesExceptAsync(userId);

            // Filter the employees based on the search term
            var employeesQuery = employees
                .Where(employee =>
                    employee.Email != null && (employee.Email.Contains(q) || employee.FirstName.Contains(q) ||
                                               employee.LastName.Contains(q))).ToList();

            var totalCount = employeesQuery.Count;

            var employeesList = employeesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(employee => new UserDto
                {
                    Id = _cypherService.Encrypt(employee.Id),
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email
                })
                .ToList();

            return Ok(new { results = employeesList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-current-user
    [HttpPost("get-current-user")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_USERS)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var userDto = new UserDto
            {
                Id = _cypherService.Encrypt(user.Id),
                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return Ok(new { success = true, user = userDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}