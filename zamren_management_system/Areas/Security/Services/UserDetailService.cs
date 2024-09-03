using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class UserDetailService : IUserDetailService
{
    private readonly AuthContext _context;
    private readonly ILogger<UserDetailService> _logger;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly ICypherService _cypherService;

    public UserDetailService(AuthContext context, ILogger<UserDetailService> logger,
        IEmailVerificationService emailVerificationService, ICypherService cypherService)
    {
        _context = context;
        _logger = logger;
        _emailVerificationService = emailVerificationService;
        _cypherService = cypherService;
    }

    public async Task<IdentityResult> CreateAsync(UserDetail userDetail)
    {
        try
        {
            await _context.UserDetails.AddAsync(userDetail);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating user detail");
            return IdentityResult.Failed(new IdentityError { Description = e.Message });
        }
    }

    public async Task<IdentityResult> UpdateAsync(UserDetail userDetail)
    {
        try
        {
            _context.UserDetails.Update(userDetail);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating user detail");
            return IdentityResult.Failed(new IdentityError { Description = e.Message });
        }
    }

    public async Task<UserDetail?> FindByIdAsync(string id)
    {
        try
        {
            return await _context.UserDetails
                .Include(ud => ud.User)
                .FirstOrDefaultAsync(ud => ud.Id == id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user detail by ID");
            return null;
        }
    }

    public async Task<UserDetail?> FindByUserIdAsync(string userId)
    {
        try
        {
            return await _context.UserDetails
                .Include(ud => ud.User)
                .FirstOrDefaultAsync(ud => ud.UserId == userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user detail by user ID");
            return null;
        }
    }

    public async Task<List<UserDetail>> FindAllAsync()
    {
        try
        {
            return await _context.UserDetails.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding all user details");
            return new List<UserDetail>();
        }
    }

    public async Task<IEnumerable<UserDetail>> FindAllByIdentityNumberAsync(string identityNumber)
    {
        try
        {
            return await _context.UserDetails.Where(ud => ud.IdentityNumber == identityNumber).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user details by identity number");
            return new List<UserDetail>();
        }
    }

    public async Task<IEnumerable<UserDetail>> FindAllByIdentityTypeAndIdentityNumberExceptByUserDetailIdAsync(
        string identityType,
        string identityNumber, string userDetailId)
    {
        try
        {
            return await _context.UserDetails
                .Where(ud =>
                    ud.IdentityType == identityType && ud.IdentityNumber == identityNumber && ud.Id != userDetailId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user details by identity type and identity number");
            return new List<UserDetail>();
        }
    }

    public async Task<IEnumerable<UserDetail>> FindAllByNextOfKinIdentityTypeAndIdentityNumberExceptByUserDetailIdAsync(
        string identityType,
        string identityNumber, string userDetailId)
    {
        try
        {
            return await _context.UserDetails
                .Where(ud =>
                    ud.NextOfKinIdentityType == identityType && ud.NextOfKinIdentityNumber == identityNumber &&
                    ud.Id != userDetailId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user details by next of kin identity type and identity number");
            return new List<UserDetail>();
        }
    }

    public async Task<IEnumerable<UserDetail>> FindAllByAlternativePhoneNumberExceptByUserDetailIdAsync(
        string alternativePhoneNumber,
        string userDetailId)
    {
        return await _context.UserDetails
            .Where(ud => ud.AlternativePhoneNumber == alternativePhoneNumber && ud.Id != userDetailId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDetail>> FindAllByNextOfKinPhoneNumberExceptByUserDetailIdAsync(
        string nextOfKinPhoneNumber,
        string userDetailId)
    {
        return await _context.UserDetails
            .Where(ud => ud.NextOfKinPhoneNumber == nextOfKinPhoneNumber && ud.Id != userDetailId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDetail>> FindAllByAlternativeEmailAddressExceptByUserDetailIdAsync(
        string alternativeEmailAddress,
        string userDetailId)
    {
        return await _context.UserDetails
            .Where(ud => ud.AlternativeEmailAddress == alternativeEmailAddress && ud.Id != userDetailId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDetail>> FindAllByNextOfKinEmailAddressExceptByUserDetailIdAsync(
        string nextOfKinEmailAddress,
        string userDetailId)
    {
        return await _context.UserDetails
            .Where(ud => ud.NextOfKinEmailAddress == nextOfKinEmailAddress && ud.Id != userDetailId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDetail>> FindAllByAlternativeEmailAddressExceptByUserDetailIdAsync(
        string alternativeEmailAddress)
    {
        try
        {
            return await _context.UserDetails.Where(ud => ud.AlternativeEmailAddress == alternativeEmailAddress)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user details by alternative email address");
            return new List<UserDetail>();
        }
    }

    //ConfirmEmailAsync method | confirm alternative email
    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code, string emailType)
    {
        try
        {
            var userDetail = await FindByUserIdAsync(user.Id);
            if (userDetail == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to verify email" });
            }

            code = _cypherService.Decrypt(code);
            var emailVerification = await _emailVerificationService.FindByTokenAsync(code);
            if (emailVerification == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to verify email" });
            }

            if (emailVerification.Token != code)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to verify email" });
            }

            if (emailVerification.IsVerified)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email already verified" });
            }

            if (emailVerification.ExpiryDate < DateTimeOffset.UtcNow)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email verification token expired" });
            }

            if (emailType == EmailVerificationType.Alternative.ToString())
            {
                userDetail.AlternativeEmailAddressConfirmed = true;
            }
            else if (emailType == EmailVerificationType.NextOfKin.ToString())
            {
                userDetail.NextOfKinEmailAddressConfirmed = true;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to verify email" });
            }

            var currentDate = DateTimeOffset.UtcNow;
            emailVerification.IsVerified = true;
            emailVerification.DateVerified = currentDate;
            emailVerification.ModifiedDate = currentDate;
            emailVerification.ModifiedByUserId = user.Id;
            await _emailVerificationService.UpdateAsync(emailVerification);

            userDetail.ModifiedDate = currentDate;
            userDetail.ModifiedByUserId = user.Id;
            await UpdateAsync(userDetail);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error confirming email");
            return IdentityResult.Failed(new IdentityError { Description = e.Message });
        }
    }

    public int CalculateProfileCompletionPercentage(UserDetail userDetail, bool isNextOfKin = false)
    {
        // Define the weight for each field. Adjust these values as needed.
        var weightPerField = isNextOfKin ? 100.0 / 29 : 100.0 / 18;

        double completionPercentage = 0;

        // Check each field. If it has a value, add the weight to the total.
        if (!string.IsNullOrEmpty(userDetail.User.FirstName)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.User.LastName)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.User.Email)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.Gender)) completionPercentage += weightPerField;
        if (userDetail.DateOfBirth.HasValue) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.ProfilePictureAttachmentId)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.IdentityType)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.IdentityNumber)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.IdentityAttachmentId)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.Country)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.City)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.AlternativePhoneNumber)) completionPercentage += weightPerField;
        if (userDetail.AlternativePhoneNumberConfirmed.HasValue && userDetail.AlternativePhoneNumberConfirmed.Value &&
            userDetail.AlternativePhoneNumberConfirmed.Value) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.AlternativeEmailAddress)) completionPercentage += weightPerField;
        if (userDetail.AlternativeEmailAddressConfirmed.HasValue && userDetail.AlternativeEmailAddressConfirmed.Value)
            completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.PhysicalAddress)) completionPercentage += weightPerField;
        if (userDetail.TermsOfUseAccepted.HasValue && userDetail.TermsOfUseAccepted.Value)
            completionPercentage += weightPerField;
        if (userDetail.PrivacyPolicyAccepted.HasValue && userDetail.PrivacyPolicyAccepted.Value)
            completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.ProofOfResidencyAttachmentId)) completionPercentage += weightPerField;

        // If isNextOfKin is true, check the 'NextOfKin' fields as well.
        if (!isNextOfKin) return (int)Math.Round(completionPercentage);

        if (!string.IsNullOrEmpty(userDetail.NextOfKinFirstName)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinLastName)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinIdentityType)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinIdentityNumber)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinIdentityAttachmentId)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinPhysicalAddress)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinGender)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinPhoneNumber)) completionPercentage += weightPerField;
        if (userDetail.NextOfKinPhoneNumberConfirmed.HasValue && userDetail.NextOfKinPhoneNumberConfirmed.Value)
            completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinCountry)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinCity)) completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinEmailAddress)) completionPercentage += weightPerField;
        if (userDetail.NextOfKinEmailAddressConfirmed.HasValue && userDetail.NextOfKinEmailAddressConfirmed.Value)
            completionPercentage += weightPerField;
        if (!string.IsNullOrEmpty(userDetail.NextOfKinProofOfResidencyAttachmentId))
            completionPercentage += weightPerField;

        // Return the total completion percentage, rounded to the nearest integer.
        return (int)Math.Round(completionPercentage);
    }
}