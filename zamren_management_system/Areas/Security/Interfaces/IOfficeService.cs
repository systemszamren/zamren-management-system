using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IOfficeService
{
    /// <summary>
    /// Creates a new office.
    /// </summary>
    /// <param name="office">The office to create.</param>
    /// <returns>The result of the creation operation.</returns>
    public Task<IdentityResult> CreateAsync(Office office);

    /// <summary>
    /// Updates an existing office.
    /// </summary>
    /// <param name="office">The office to update.</param>
    /// <returns>The result of the update operation.</returns>
    public Task<IdentityResult> UpdateAsync(Office office);

    /// <summary>
    /// Retrieves an office by its ID.
    /// </summary>
    /// <param name="officeId">The ID of the office.</param>
    /// <returns>The office if found, null otherwise.</returns>
    public Task<Office?> FindByIdAsync(string officeId);

    /// <summary>
    /// Retrieves all offices.
    /// </summary>
    /// <returns>A collection of all offices.</returns>
    public Task<List<Office>> FindAllAsync();

    /// <summary>
    /// Deletes an office.
    /// </summary>
    /// <param name="office">The office to delete.</param>
    /// <returns>The result of the deletion operation.</returns>
    public Task<IdentityResult> DeleteAsync(Office office);

    /// <summary>
    /// Retrieves all users in a specific office.
    /// </summary>
    /// <param name="officeId">The ID of the office.</param>
    /// <returns>A collection of all users in the specified office.</returns>
    public Task<List<UserOffice>> FindUsersAsync(string officeId);

    /// <summary>
    /// Adds a user to an office.
    /// </summary>
    /// <param name="userOffice">The user and office information.</param>
    /// <returns>The result of the addition operation.</returns>
    public Task<IdentityResult> AddUserAsync(UserOffice userOffice);

    /// <summary>
    /// Checks if an office with the given name exists.
    /// </summary>
    /// <param name="name">The name of the office.</param>
    /// <returns>True if the office exists, false otherwise.</returns>
    public Task<bool> OfficeNameExistsAsync(string name);

    //ExistsExceptAsync
    public Task<bool> OfficeNameExistsExceptAsync(string name, string officeId);

    /// <summary>
    /// Checks if a user is in a specific office.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="officeId">The ID of the office.</param>
    /// <returns>True if the user is in the office, false otherwise.</returns>
    public Task<bool> IsInOfficeAsync(string userId, string officeId);

    /// <summary>
    /// Retrieves all users not in a specific office.
    /// </summary>
    /// <param name="officeId">The ID of the office.</param>
    /// <returns>A collection of all users not in the specified office.</returns>
    public Task<ICollection<ApplicationUser>> FindUsersNotInOfficeAsync(string officeId);

    /// <summary>
    /// Removes a user from an office.
    /// </summary>
    /// <param name="userOffice">The user and office information.</param>
    /// <returns>The result of the removal operation.</returns>
    public Task<IdentityResult> RemoveUserAsync(UserOffice userOffice);

    /// <summary>
    ///     Retrieves all offices except the one with the given ID.
    /// </summary> 
    /// <param name="officeId"></param>
    /// <returns> A collection of all offices except the one with the given ID.</returns>
    Task<List<Office>> FindAllExceptAsync(string officeId);


    /// <summary>
    ///     Retrieves all offices except the one with the given ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="officeId"></param>
    /// <returns> Office if found, null otherwise.</returns>
    Task<UserOffice?> GetUserOfficeAsync(string userId, string officeId);

    /// <summary>
    ///     Retrieve a user office object by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns> UserOffice if found, null otherwise.</returns>
    public Task<UserOffice?> FindUserOfficeByIdAsync(string id);

    /// <summary>
    ///     Update a user office object.
    /// </summary>
    /// <param name="userOffice"></param>
    /// <returns></returns>
    public Task<IdentityResult> UpdateUserOfficeAsync(UserOffice userOffice);

    /// <summary>
    ///     Retrieve all offices in a department.
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    Task<List<Office>> FindByDepartmentIdAsync(string departmentId);

    /// <summary>
    ///     Retrieve a user office object by user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<UserOffice?> FindOfficeByUserIdAsync(string userId);
}