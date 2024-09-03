using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Common.Responses;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface IWorkflowService
{
    /// <summary>
    ///     Generates a reference number for a specific module.
    /// </summary>
    /// <param name="moduleCode"> The code of the module.</param>
    /// <returns> The generated reference number.</returns>
    Task<string> GenerateReferenceNumber(string moduleCode);

    /// <summary>
    ///     Initiates a workflow task.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="currentUserId"></param>
    /// <param name="processId"></param>
    /// <param name="comment"></param>
    /// <param name="systemAttachments"></param>
    /// <param name="parentReference"></param>
    /// <returns> The initiated workflow task and the response.</returns>
    Task<(WkfTask? task, CustomIdentityResult response)> InitiateWorkflowTask(
        string reference,
        string currentUserId,
        string processId,
        string? comment = null,
        List<SystemAttachment>? systemAttachments = null,
        string? parentReference = null
    );

    /// <summary>
    ///     Approves a workflow task.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="currentUserId"></param>
    /// <param name="comment"></param>
    /// <param name="systemAttachments"></param>
    /// <returns> The approved workflow task and the response.</returns>
    Task<(WkfTask? task, CustomIdentityResult response)> ApproveWorkflowTask(
        string reference,
        string currentUserId,
        string? comment = null,
        List<SystemAttachment>? systemAttachments = null
    );

    /// <summary>
    ///     Sends back a workflow task.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="currentUserId"></param>
    /// <param name="comment"></param>
    /// <param name="systemAttachments"></param>
    /// <returns> The sent back workflow task and the response.</returns>
    Task<(WkfTask? task, CustomIdentityResult response)> SendBackWorkflowTask(
        string reference,
        string currentUserId,
        string? comment = null,
        List<SystemAttachment>? systemAttachments = null
    );

    /// <summary>
    ///     Reassigns a workflow task.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="nextActioningUserId"></param>
    /// <param name="currentUserId"></param>
    /// <returns> The reassigned workflow task and the response.</returns>
    Task<(WkfTask? task, CustomIdentityResult response)> ReassignWorkflowTask(
        string reference,
        string nextActioningUserId,
        string currentUserId
    );

    /// <summary>
    ///     Closes a workflow task.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="currentUserId"></param>
    /// <returns> The closed workflow task and the response.</returns>
    Task<(WkfTask? task, CustomIdentityResult response)> CloseWorkflowTask(
        string reference,
        string currentUserId
    );

    /// <summary>
    ///     Reopens a workflow task.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="currentUserId"></param>
    /// <returns> The reopened workflow task and the response.</returns>
    Task<(WkfTask? task, CustomIdentityResult response)> ReopenWorkflowTask(
        string reference,
        string currentUserId
    );
}