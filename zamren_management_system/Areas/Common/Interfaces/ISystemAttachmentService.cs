using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Models;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface ISystemAttachmentService
{
    /// <summary>
    ///     Saves a system attachment to the database.
    /// </summary>
    /// <param name="systemAttachment"></param>
    /// <returns></returns>
    Task<IdentityResult> CreateAsync(SystemAttachment systemAttachment);

    /// <summary>
    ///     Saves a list of system attachments to the database.
    /// </summary>
    /// <param name="systemAttachments"></param>
    /// <returns></returns>
    Task<IdentityResult> CreateAsync(IEnumerable<SystemAttachment> systemAttachments);

    /// <summary>
    ///     Updates a system attachment in the database.
    /// </summary>
    /// <param name="systemAttachment"></param>
    /// <returns></returns>
    Task<IdentityResult> UpdateAsync(SystemAttachment systemAttachment);

    /// <summary>
    ///     Deletes a system attachment from the database.
    /// </summary>
    /// <param name="systemAttachment"></param>
    /// <returns></returns>
    Task<IdentityResult> DeleteAsync(SystemAttachment systemAttachment);

    /// <summary>
    ///     Gets a system attachment by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<SystemAttachment?> FindByIdAsync(string id);

    /// <summary>
    ///     Gets system attachments by user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<SystemAttachment>> GetByUserIdAsync(string userId);

    /// <summary>
    ///     Gets all system attachments.
    /// </summary>
    /// <returns> A list of system attachments. </returns>
    Task<IEnumerable<SystemAttachment>> FindAllAsync();
}