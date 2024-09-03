using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IUserDetailService
{
    /// <summary>
    /// Creates a new user detail.
    /// </summary>
    /// <param name="userDetail">The user detail to create.</param>
    /// <returns>The result of the creation operation.</returns>
    public Task<IdentityResult> CreateAsync(UserDetail userDetail);

    /// <summary>
    /// Updates an existing user detail.
    /// </summary>
    /// <param name="userDetail">The user detail to update.</param>
    /// <returns>The result of the update operation.</returns>
    public Task<IdentityResult> UpdateAsync(UserDetail userDetail);

    /// <summary>
    /// Retrieves a user detail by its ID.
    /// </summary>
    /// <param name="id">The ID of the user detail.</param>
    /// <returns>The user detail if found, null otherwise.</returns>
    public Task<UserDetail?> FindByIdAsync(string id);

    /// <summary>
    ///  Retrieves a user detail by its user ID.
    /// </summary>
    /// <param name="userId"> The user ID of the user detail.</param>
    /// <returns> The user detail if found, null otherwise.</returns>
    public Task<UserDetail?> FindByUserIdAsync(string userId);

    /// <summary>
    /// Retrieves all user details.
    /// </summary>
    /// <returns>A list of all user details.</returns>
    public Task<List<UserDetail>> FindAllAsync();

    /// <summary>
    ///     This method is used to find all user details by identity number
    /// </summary>
    /// <param name="identityNumber"> The identity number to search for </param>
    /// <returns> IEnumerable<UserDetail/> </returns>
    public Task<IEnumerable<UserDetail>> FindAllByIdentityNumberAsync(string identityNumber);

    /// <summary>
    ///     This method is used to find all user details by alternative email address
    /// </summary>
    /// <param name="alternativeEmailAddress"> The alternative email address to search for </param>
    /// <returns> IEnumerable<UserDetail/> </returns>
    public Task<IEnumerable<UserDetail>> FindAllByAlternativeEmailAddressExceptByUserDetailIdAsync(string alternativeEmailAddress);

    /// <summary>
    ///     This method is used to find all user details by identity type and identity number
    /// </summary>
    /// <param name="identityType"> The identity type to search for </param>
    /// <param name="identityNumber"> The identity number to search for </param>
    /// <param name="userDetailId"> The user detail ID to exclude during the search </param>
    /// <returns> IEnumerable<UserDetail/> </returns>
    public Task<IEnumerable<UserDetail>> FindAllByIdentityTypeAndIdentityNumberExceptByUserDetailIdAsync(
        string identityType,
        string identityNumber, string userDetailId);

    /// <summary>
    ///   This method is used to find all users with the specified email address
    /// </summary>
    /// <param name="alternativeEmailAddress"> The email address to search for </param>
    /// <param name="userDetailId"> The user detail id to exclude from the search </param>
    /// <returns> IEnumerable<UserDetail/> </returns>
    public Task<IEnumerable<UserDetail>> FindAllByAlternativeEmailAddressExceptByUserDetailIdAsync(string alternativeEmailAddress,
        string userDetailId);

    /// <summary>
    ///     This method is used to find all users with the specified next of kin email address
    /// </summary>
    /// <param name="nextOfKinEmailAddress"></param>
    /// <param name="userDetailId"></param>
    /// <returns></returns>
    public Task<IEnumerable<UserDetail>> FindAllByNextOfKinEmailAddressExceptByUserDetailIdAsync(
        string nextOfKinEmailAddress,
        string userDetailId);


    /// <summary>
    ///     This method is used to find all users with the specified phone number
    /// </summary>
    /// <param name="phoneNumber"> The phone number to search for </param>
    /// <param name="userDetailId"> The user detail id to exclude from the search </param>
    /// <returns> IEnumerable<UserDetail/> </returns>
    public Task<IEnumerable<UserDetail>> FindAllByAlternativePhoneNumberExceptByUserDetailIdAsync(string phoneNumber,
        string userDetailId);
    
    /// <summary>
    ///   This method is used to find all users with the specified next of kin phone number
    /// </summary>
    /// <param name="nextOfKinPhoneNumber"></param>
    /// <param name="userDetailId"></param>
    /// <returns></returns>
    public Task<IEnumerable<UserDetail>> FindAllByNextOfKinPhoneNumberExceptByUserDetailIdAsync(string nextOfKinPhoneNumber,
        string userDetailId);

    /// <summary>
    ///     This method is used to find all users with the specified identity type and identity number
    /// </summary>
    /// <param name="identityType"></param>
    /// <param name="identityNumber"></param>
    /// <param name="userDetailId"></param>
    /// <returns> IEnumerable<UserDetail/> </returns>
    public Task<IEnumerable<UserDetail>> FindAllByNextOfKinIdentityTypeAndIdentityNumberExceptByUserDetailIdAsync(
        string identityType,
        string identityNumber, string userDetailId);

    
    /// <summary>
    ///     Confirms the email of a user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="code"></param>
    /// <param name="emailType"></param>
    /// <returns> The result of the confirmation operation </returns>
    public Task<IdentityResult>  ConfirmEmailAsync(ApplicationUser user, string code, string emailType);

    /// <summary>
    ///     This method is used to calculate the profile completion percentage of a user
    /// </summary>
    /// <param name="userDetail"></param>
    /// <param name="isNextOfKin"></param>
    /// <returns></returns>
    public int CalculateProfileCompletionPercentage(UserDetail userDetail, bool isNextOfKin = false);
}