using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface ITaskService
{
    /// <summary>
    ///     Create a task
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task<IdentityResult> CreateAsync(WkfTask task);

    /// <summary>
    ///    Update a task
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task<IdentityResult> UpdateAsync(WkfTask task);

    /// <summary>
    ///     Delete a task
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task<IdentityResult> DeleteAsync(WkfTask task);

    /// <summary>
    ///     Find tasks by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> A list of tasks</returns>
    Task<List<WkfTask>> FindByUserIdAsync(string userId);

    /// <summary>
    ///     Find a task by its reference
    /// </summary>
    /// <param name="reference"></param>
    /// <returns> A task</returns>
    Task<WkfTask?> FindByReferenceAsync(string reference);

    /// <summary>
    ///     Find all tasks
    /// </summary>
    /// <returns> A list of tasks</returns>
    Task<List<WkfTask>> FindAllAsync();

    /// <summary>
    ///     Find a task by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns> A task</returns>
    Task<WkfTask?> FindByIdAsync(string id);

    /// <summary>
    ///     Find tasks that are part of a process
    /// </summary>
    /// <param name="processId"></param>
    /// <returns> A list of tasks</returns>
    Task<List<WkfTask>> FindByProcessIdAsync(string processId);

    /// <summary>
    ///     Find tasks that are currently at a step
    /// </summary>
    /// <param name="stepId"></param>
    /// <returns> A list of tasks</returns>
    Task<List<WkfTask>> FindByCurrentStepIdAsync(string stepId);

    /// <summary>
    ///     Find tasks that were started by a user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> A list of tasks</returns>
    Task<List<WkfTask>> FindByTaskStartedByUserIdAsync(string userId);

    /// <summary>
    ///     Find tasks that are currently being actioned by a user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> A list of tasks</returns>
    Task<List<WkfTask>> FindByCurrentActioningUserIdAsync(string userId);

    /// <summary>
    ///     Get all child tasks of a task
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns> A list of child tasks</returns>
    Task<List<WkfTask>> GetChildTasksAsync(string taskId);
}