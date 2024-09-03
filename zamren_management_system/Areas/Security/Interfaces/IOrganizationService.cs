using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IOrganizationService
{
    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <param name="organization">The organization to create.</param>
    /// <returns>The result of the creation operation.</returns>
    public Task<IdentityResult> CreateAsync(Organization organization);

    /// <summary>
    /// Updates an existing organization.
    /// </summary>
    /// <param name="organization">The organization to update.</param>
    /// <returns>The result of the update operation.</returns>
    public Task<IdentityResult> UpdateAsync(Organization organization);

    /// <summary>
    /// Retrieves an organization by its ID.
    /// </summary>
    /// <param name="id">The ID of the organization.</param>
    /// <returns>The organization if found, null otherwise.</returns>
    public Task<Organization?> FindByIdAsync(string id);

    /// <summary>
    /// Retrieves all organizations.
    /// </summary>
    /// <returns>A list of all organizations.</returns>
    public Task<List<Organization>> FindAllAsync();

    /// <summary>
    /// Retrieves all departments in a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>A collection of all departments in the specified organization.</returns>
    public Task<ICollection<Department>> FindDepartmentsAsync(string organizationId);

    /// <summary>
    /// Retrieves all offices in a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>A collection of all offices in the specified organization.</returns>
    public Task<ICollection<Office>> FindOfficesAsync(string organizationId);

    /// <summary>
    /// Retrieves all users in a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>A collection of all users in the specified organization.</returns>
    public Task<ICollection<ApplicationUser>> FindUsersAsync(string organizationId);

    /// <summary>
    /// Retrieves all branches in a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>A collection of all branches in the specified organization.</returns>
    public Task<ICollection<Branch>> FindBranchesAsync(string organizationId);

    /// <summary>
    /// Checks if an organization with the given name exists.
    /// </summary>
    /// <param name="name">The name of the organization.</param>
    /// <returns>True if the organization exists, false otherwise.</returns>
    public Task<bool> OrganizationNameExistsAsync(string name);

    /// <summary>
    ///  Checks if an organization with the given name exists, except for the organization with the given ID.
    ///  </summary>
    ///  <param name="name">The name of the organization.</param>
    ///  <param name="id">The ID of the organization to exclude from the check.</param>
    ///  <returns>True if the organization exists, false otherwise.</returns>
    public Task<bool> OrganizationNameExistsExceptAsync(string name, string id);

    /// <summary>
    /// Checks if an organization with the given ID exists.
    /// </summary>
    /// <param name="id">The ID of the organization.</param>
    /// <returns>True if the organization exists, false otherwise.</returns>
    public Task<bool> OrganizationIdExistsAsync(string id);

    /// <summary>
    /// Deletes an organization.
    /// </summary>
    /// <param name="organization">The organization to delete.</param>
    /// <returns>The result of the deletion operation.</returns>
    public Task<IdentityResult> DeleteAsync(Organization organization);

    /// <summary>
    ///  Retrieves all organizations except the one with the given ID.
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    public Task<List<Organization>> FindAllExceptAsync(string organizationId);
}