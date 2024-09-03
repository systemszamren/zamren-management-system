using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IDepartmentService
{
    /// <summary>
    /// Creates a new department.
    /// </summary>
    /// <param name="department">The department to create.</param>
    /// <returns>The result of the creation operation.</returns>
    public Task<IdentityResult> CreateAsync(Department department);

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    /// <param name="department">The department to update.</param>
    /// <returns>The result of the update operation.</returns>
    public Task<IdentityResult> UpdateAsync(Department department);

    /// <summary>
    /// Deletes a department.
    /// </summary>
    /// <param name="department">The department to delete.</param>
    /// <returns>The result of the deletion operation.</returns>
    public Task<IdentityResult> DeleteAsync(Department department);

    /// <summary>
    /// Retrieves a department by its ID.
    /// </summary>
    /// <param name="departmentId">The ID of the department.</param>
    /// <returns>The department if found, null otherwise.</returns>
    public Task<Department?> FindByIdAsync(string departmentId);

    /// <summary>
    /// Retrieves all departments.
    /// </summary>
    /// <returns>A collection of all departments.</returns>
    public Task<List<Department>> FindAllAsync();

    /// <summary>
    /// Retrieves all offices in a specific department.
    /// </summary>
    /// <param name="departmentId">The ID of the department.</param>
    /// <returns>A collection of all offices in the specified department.</returns>
    public Task<ICollection<Office>> FindOfficesAsync(string departmentId);

    /// <summary>
    /// Retrieves all users in a specific department.
    /// </summary>
    /// <param name="departmentId">The ID of the department.</param>
    /// <returns>A collection of all users in the specified department.</returns>
    public Task<ICollection<ApplicationUser>> FindUsersAsync(string departmentId);

    /// <summary>
    /// Checks if a department with the given name exists.
    /// </summary>
    /// <param name="name">The name of the department.</param>
    /// <returns>True if the department exists, false otherwise.</returns>
    public Task<bool> DepartmentNameExistsAsync(string name);
    
    /// <summary>
    ///  Checks if a department with the given name exists, except for the department with the given ID.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="departmentId"></param>
    /// <returns> True if the department exists, false otherwise.</returns>
    public Task<bool> DepartmentNameExistsExceptAsync(string name, string departmentId);
    
    /// <summary>
    /// Checks if a department with the given ID exists.
    /// </summary>
    /// <param name="departmentId">The ID of the department.</param>
    /// <returns>True if the department exists, false otherwise.</returns>
    public Task<bool> DepartmentIdExistsAsync(string departmentId);

    /// <summary>
    ///     Retrieves all departments except for the department with the given ID.
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns> A collection of all departments except for the department with the given ID.</returns>
    Task<List<Department>> FindAllExceptAsync(string departmentId);

    /// <summary>
    ///     Retrieves all departments in a specific branch.
    /// </summary>
    /// <param name="branchId"></param>
    /// <returns></returns>
    Task<ICollection<Department>> FindByBranchIdAsync(string branchId);
}