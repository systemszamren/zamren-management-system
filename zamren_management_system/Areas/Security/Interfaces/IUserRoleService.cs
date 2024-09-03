using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IUserRoleService
{
    /// <summary>
    /// Adds a role to a user.
    /// </summary>
    /// <param name="userRole"> The user and role information. </param>
    /// <returns> IdentityResult </returns>
    public Task<IdentityResult> CreateAsync(ApplicationUserRole userRole);

    /// <summary>
    ///  Finds a user role by user id and role id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roleId"></param>
    /// <returns> ApplicationUserRole </returns>
    public Task<ApplicationUserRole?> FindByUserIdAndRoleIdAsync(string userId, string roleId);

    /// <summary>
    ///     Gets all user-role mappings for a given role.
    /// </summary>
    /// <param name="role"></param>
    /// <returns> List of UserRoleDto </returns>
    public Task<List<UserRoleDto>> GetRoleUsers(ApplicationRole role);

    /// <summary>
    ///     Finds a user role by id.
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <returns> ApplicationUserRole </returns>
    public Task<ApplicationUserRole?> FindByIdAsync(string uniqueId);

    
    /// <summary>
    ///     Updates a user role.
    /// </summary>
    /// <param name="userRole"></param>
    /// <returns> IdentityResult </returns>
    public Task<IdentityResult> UpdateAsync(ApplicationUserRole userRole);

    /// <summary>
    ///     Gets all roles for a given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> List of UserRoleDto </returns>
    public Task<List<UserRoleDto>> GetUserRoles(string userId);
}