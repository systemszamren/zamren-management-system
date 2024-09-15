using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Responses;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface IStepService
{
    /// <summary>
    ///     Create a new step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    Task<IdentityResult> CreateAsync(WkfProcessStep step);

    /// <summary>
    ///     Update an existing step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    Task<IdentityResult> UpdateAsync(WkfProcessStep step);

    /// <summary>
    ///     Delete an existing step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    Task<IdentityResult> DeleteAsync(WkfProcessStep step);

    /// <summary>
    ///     Find a step by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> FindByIdAsync(string id);

    /// <summary>
    ///     Find a step by its name in a process
    /// </summary>
    /// <param name="name"></param>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> GetByNameInProcessAsync(string name, string processId);

    /// <summary>
    ///     Find a step by its name in a process except the current step
    /// </summary>
    /// <param name="name"></param>
    /// <param name="processId"></param>
    /// <param name="stepId"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> GetByNameInProcessExceptAsync(string name, string processId, string stepId);

    /// <summary>
    ///     Find a step by its name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> FindByNameAsync(string name);

    /// <summary>
    ///     Find a step by its name except the current step
    /// </summary>
    /// <param name="name"></param>
    /// <param name="stepId"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> FindByNameExceptAsync(string name, string stepId);

    /// <summary>
    ///     Find a step by process id
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<List<WkfProcessStep>> FindByProcessIdAsync(string processId);

    /// <summary>
    ///     Find initial step in a process
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> FindInitialStepAsync(string processId);

    /// <summary>
    ///     Find final step in a process
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> FindFinalStepAsync(string processId);

    /// <summary>
    ///     Check if a step marked as a next step for another step
    /// </summary>
    /// <param name="stepId"></param>
    /// <returns></returns>
    Task<bool> IsNextStepAsync(string stepId);

    /// <summary>
    ///     Check if a step marked as a previous step for another step
    /// </summary>
    /// <param name="stepId"></param>
    /// <returns></returns>
    Task<bool> IsPreviousStepAsync(string stepId);

    /// <summary>
    ///     Get ordered steps in a process
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<(List<WkfProcessStep> steps, CustomIdentityResult response)> GetOrderedStepsAsync(string processId);

    /// <summary>
    ///     Find all steps in a process
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<ICollection<WkfProcessStep>> FindAllByProcessIdAsync(string processId);

    /// <summary>
    ///     Find employees not in a step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    Task<IEnumerable<ApplicationUser>> FindEmployeesNotInStepAsync(WkfProcessStep step);

    /// <summary>
    ///     Find steps in a process except the current step
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="stepId"></param>
    /// <returns></returns>
    Task<ICollection<WkfProcessStep>> FindAllByProcessIdExceptAsync(string processId, string stepId);

    /// <summary>
    ///     Find actioning users (employees) in a step
    /// </summary>
    /// <param name="step"></param>
    /// <returns> List of users to perform an action in a step </returns>
    Task<(IEnumerable<ApplicationUser> Users, CustomIdentityResult response)> FindActioningUsersAsync(
        WkfProcessStep step);

    /// <summary>
    ///     Get first step in a process
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    Task<WkfProcessStep?> GetFirstStepAsync(string processId);

    /// <summary>
    ///     Get actioning user in a step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    Task<ApplicationUser?> FindActioningUserAsync(WkfProcessStep step);
}