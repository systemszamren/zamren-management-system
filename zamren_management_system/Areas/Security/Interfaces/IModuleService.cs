using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IModuleService
{
    /// <summary>
    /// Retrieves all modules.
    /// </summary>
    /// <returns>A collection of all modules.</returns>
    Task<ICollection<Module>> FindAllAsync();

    /// <summary>
    /// Retrieves all privileges for a specific module.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <returns>A collection of all privileges for the specified module.</returns>
    Task<IEnumerable<ModulePrivilege?>> FindPrivilegesAsync(string moduleId);

    /// <summary>
    /// Checks if a module with the given name exists.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    /// <returns>True if the module exists, false otherwise.</returns>
    Task<bool> ModuleNameExistsAsync(string name);

    //ExistsExceptAsync
    public Task<bool> ModuleNameExistsExceptAsync(string name, string id);

    /// <summary>
    /// Checks if a module with the given code exists.
    /// </summary>
    /// <param name="code">The code of the module.</param>
    /// <returns>True if the module exists, false otherwise.</returns>
    Task<bool> ModuleCodeExistsAsync(string code);

    //ExistsExceptAsync
    public Task<bool> ModuleCodeExistsExceptAsync(string code, string id);

    /// <summary>
    /// Creates a new module.
    /// </summary>
    /// <param name="module">The module to create.</param>
    /// <returns>The result of the creation operation.</returns>
    Task<IdentityResult> CreateAsync(Module module);

    /// <summary>
    /// Retrieves a module by its ID.
    /// </summary>
    /// <param name="id">The ID of the module.</param>
    /// <returns>The module if found, null otherwise.</returns>
    Task<Module?> FindByIdAsync(string id);

    /// <summary>
    /// Updates an existing module.
    /// </summary>
    /// <param name="module">The module to update.</param>
    /// <returns>The result of the update operation.</returns>
    Task<IdentityResult> UpdateAsync(Module module);

    /// <summary>
    /// Deletes a module.
    /// </summary>
    /// <param name="module">The module to delete.</param>
    /// <returns>The result of the deletion operation.</returns>
    Task<IdentityResult> DeleteAsync(Module module);

    /// <summary>
    /// Creates multiple modules.
    /// </summary>
    /// <param name="modules">The modules to create.</param>
    /// <returns>The result of the creation operation.</returns>
    Task<IdentityResult> CreateAsync(IEnumerable<Module> modules);

    /// <summary>
    /// Adds a privilege to a module.
    /// </summary>
    /// <param name="modulePrivilege">The privilege to add to the module.</param>
    /// <returns>The result of the addition operation.</returns>
    Task<IdentityResult> AddPrivilegeAsync(ModulePrivilege modulePrivilege);

    /// <summary>
    /// Retrieves all privileges not in a specific module.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <returns>A collection of all privileges not in the specified module.</returns>
    Task<IEnumerable<Privilege>> FindNotInModuleAsync(string moduleId);

    /// <summary>
    /// Checks if a privilege is in a specific module.
    /// </summary>
    /// <param name="privilegeId">The ID of the privilege.</param>
    /// <param name="moduleId">The ID of the module.</param>
    /// <returns>True if the privilege is in the module, false otherwise.</returns>
    Task<bool> IsInModuleAsync(string privilegeId, string moduleId);

    /// <summary>
    /// Removes a privilege from a module.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <param name="privilegeId">The ID of the privilege to remove.</param>
    /// <returns>The result of the removal operation.</returns>
    Task<IdentityResult> RemovePrivilegeAsync(string moduleId, string privilegeId);

    /// <summary>
    ///     Retrieves all modules except the one with the specified ID.
    /// </summary>
    /// <param name="moduleId"> The ID of the module to exclude.</param>
    /// <returns> A collection of all modules except the specified module.</returns>
    Task<List<Module>> FindAllExceptAsync(string moduleId);
}