using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IPrivilegeService
{
    /// <summary>
    /// Retrieves the names of all privileges associated with a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A set of privilege names.</returns>
    Task<HashSet<string>> FindNamesByUserAsync(string userId);

    /// <summary>
    /// Creates a new privilege.
    /// </summary>
    /// <param name="privilege">The privilege to create.</param>
    /// <returns>The result of the creation operation.</returns>
    public Task<IdentityResult> CreateAsync(Privilege privilege);

    /// <summary>
    /// Checks if a privilege with the given name exists.
    /// </summary>
    /// <param name="name">The name of the privilege.</param>
    /// <returns>True if the privilege exists, false otherwise.</returns>
    public Task<bool> PrivilegeNameExistsAsync(string name);

    //ExistsExceptAsync
    public Task<bool> PrivilegeNameExistsExceptAsync(string name, string id);

    /// <summary>
    /// Retrieves all privileges.
    /// </summary>
    /// <returns>A collection of all privileges.</returns>
    public Task<ICollection<Privilege>> FindAllAsync();

    /// <summary>
    /// Retrieves a privilege by its ID.
    /// </summary>
    /// <param name="id">The ID of the privilege.</param>
    /// <returns>The privilege if found, null otherwise.</returns>
    public Task<Privilege?> FindByIdAsync(string id);

    /// <summary>
    /// Retrieves all roles associated with a specific privilege.
    /// </summary>
    /// <param name="id">The ID of the privilege.</param>
    /// <returns>A collection of all roles associated with the specified privilege.</returns>
    public Task<IEnumerable<ApplicationRole>> FindRolesAsync(string id);

    /// <summary>
    /// Deletes a privilege.
    /// </summary>
    /// <param name="privilege">The privilege to delete.</param>
    /// <returns>The result of the deletion operation.</returns>
    public Task<IdentityResult> DeleteAsync(Privilege privilege);

    /// <summary>
    /// Removes a privilege from a role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <param name="privilegeId">The ID of the privilege.</param>
    /// <returns>The result of the removal operation.</returns>
    public Task<IdentityResult> RemoveFromRoleAsync(string roleId, string privilegeId);

    /// <summary>
    /// Updates an existing privilege.
    /// </summary>
    /// <param name="privilege">The privilege to update.</param>
    /// <returns>The result of the update operation.</returns>
    public Task<IdentityResult> UpdateAsync(Privilege privilege);

    /// <summary>
    /// Adds a privilege to a role.
    /// </summary>
    /// <param name="privilegeRole">The privilege and role information.</param>
    /// <returns>The result of the addition operation.</returns>
    public Task<IdentityResult> AddToRoleAsync(RolePrivilege privilegeRole);

    /// <summary>
    /// Retrieves all privileges associated with a specific role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A collection of all privileges associated with the specified role.</returns>
    public Task<IEnumerable<Privilege?>> FindInRoleAsync(string roleId);

    /// <summary>
    /// Retrieves all privileges not associated with a specific role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A collection of all privileges not associated with the specified role.</returns>
    public Task<IEnumerable<Privilege?>> FindNotInRoleAsync(string roleId);

    /// <summary>
    /// Checks if a privilege is associated with a specific role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <param name="privilegeId">The ID of the privilege.</param>
    /// <returns>True if the privilege is associated with the role, false otherwise.</returns>
    public Task<bool> IsInRoleAsync(string roleId, string privilegeId);

    /// <summary>
    ///     Retrieves all privileges associated with a specific module.
    /// </summary>
    /// <param name="moduleId"></param>
    /// <returns></returns>
    public Task<ICollection<Privilege>> FindInModuleAsync(string moduleId);

    /// <summary>
    ///     Retrieves all privileges associated with a specific module.
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public Task<ICollection<Privilege>> FindByModuleNameAsync(string moduleName);

    /// <summary>
    ///     Retrieves all module privileges.
    /// </summary>
    /// <returns></returns>
    Task<List<Privilege>> GetAllModulePrivilegeAsync();
}