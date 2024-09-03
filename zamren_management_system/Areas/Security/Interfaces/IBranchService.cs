using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IBranchService
{
    /// <summary>
    /// Creates a new branch.
    /// </summary>
    /// <param name="branch">The branch to create.</param>
    /// <returns>The result of the creation operation.</returns>
    public Task<IdentityResult> CreateAsync(Branch branch);

    /// <summary>
    /// Updates an existing branch.
    /// </summary>
    /// <param name="branch">The branch to update.</param>
    /// <returns>The result of the update operation.</returns>
    public Task<IdentityResult> UpdateAsync(Branch branch);

    /// <summary>
    /// Deletes a branch.
    /// </summary>
    /// <param name="branch">The branch to delete.</param>
    /// <returns>The result of the deletion operation.</returns>
    public Task<IdentityResult> DeleteAsync(Branch branch);

    /// <summary>
    /// Retrieves a branch by its ID.
    /// </summary>
    /// <param name="branchId">The ID of the branch.</param>
    /// <returns>The branch if found, null otherwise.</returns>
    public Task<Branch?> FindByIdAsync(string branchId);

    /// <summary>
    /// Retrieves all branches.
    /// </summary>
    /// <returns>A list of all branches.</returns>
    public Task<List<Branch>> FindAllAsync();

    /// <summary>
    /// Retrieves all departments in a specific branch.
    /// </summary>
    /// <param name="branchId">The ID of the branch.</param>
    /// <returns>A collection of all departments in the specified branch.</returns>
    public Task<ICollection<Department>> FindDepartmentsAsync(string branchId);

    /// <summary>
    /// Retrieves all offices in a specific branch.
    /// </summary>
    /// <param name="branchId">The ID of the branch.</param>
    /// <returns>A collection of all offices in the specified branch.</returns>
    public Task<ICollection<Office>> FindOfficesAsync(string branchId);

    /// <summary>
    /// Retrieves all users in a specific department.
    /// </summary>
    /// <param name="departmentId">The ID of the department.</param>
    /// <returns>A collection of all users in the specified department.</returns>
    public Task<ICollection<ApplicationUser>> FindUsersAsync(string departmentId);

    /// <summary>
    /// Checks if a branch with the given name exists.
    /// </summary>
    /// <param name="name">The name of the branch.</param>
    /// <returns>True if the branch exists, false otherwise.</returns>
    public Task<bool> BranchNameExistsAsync(string name);

    /// <summary>
    ///     Checks if a branch with the given name exists, except for the branch with the given ID.
    /// </summary>
    ///   <param name="name">The name of the branch.</param>
    ///  <param name="branchId">The ID of the branch to exclude from the check.</param>
    ///  <returns>True if the branch exists, false otherwise.</returns>
    public Task<bool> BranchNameExistsExceptAsync(string name, string branchId);

    /// <summary>
    /// Checks if a branch with the given ID exists.
    /// </summary>
    /// <param name="branchId">The ID of the branch.</param>
    /// <returns>True if the branch exists, false otherwise.</returns>
    public Task<bool> BranchIdExistsAsync(string branchId);

    /// <summary>
    ///     Retrieves all branches except for the one with the given ID.
    /// </summary>
    /// <param name="branchId"></param>
    /// <returns> A list of all branches except for the one with the given ID.</returns>
    public Task<List<Branch>> FindAllExceptAsync(string branchId);

    /// <summary>
    ///     Retrieves all branches in a specific organization.
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    Task<ICollection<Branch>> FindByOrganizationIdAsync(string organizationId);
}