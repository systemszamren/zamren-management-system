using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IUserService
{
    /// <summary>
    ///     This method is used to find all users except the user with the specified id
    /// </summary>
    /// <param name="userId"> The user id to exclude from the search </param>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllExceptByIdAsync(string userId);

    /// <summary>
    ///     This method is used to find all users except the user with the specified email
    /// </summary>
    /// <param name="email"> The email to exclude from the search </param>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllExceptByEmailAsync(string email);

    /// <summary>
    ///     This method is used to find all users with the specified supervisor id
    /// </summary>
    /// <param name="supervisorUserId"> The supervisor user id to search for </param>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllBySupervisorIdAsync(string supervisorUserId);

    /// <summary>
    ///     This method is used to find all users by phone number except the user with the specified id
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="userId"></param>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllByPhoneNumberExceptByUserIdAsync(string phoneNumber,
        string userId);

    /// <summary>
    ///     This method is used to find all users by email address except the user with the specified id
    /// </summary>
    /// <param name="email"></param>
    /// <param name="userId"></param>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllByEmailAddressExceptByUserIdAsync(string email,
        string userId);

    /// <summary>
    ///     This method is used to find all employees
    /// </summary>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllEmployeesAsync();

    /// <summary>
    ///     This method is used to find all employees except the user with the specified id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> IEnumerable<ApplicationUser/> </returns>
    public Task<IEnumerable<ApplicationUser>> FindAllEmployeesExceptAsync(string userId);
}