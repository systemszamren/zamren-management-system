using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Interfaces;

public interface IPasswordHistoryService
{
    /// <summary>
    /// Adds a password history.
    /// </summary>
    /// <param name="passwordHistory"> The password history information. </param>
    /// <returns> IdentityResult </returns>
    public Task<IdentityResult> CreateAsync(PasswordHistory passwordHistory);

    /// <summary>
    ///  Finds a password history by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns> PasswordHistory </returns>
    public Task<PasswordHistory?> FindByIdAsync(string id);

    /// <summary>
    ///     Gets all password histories for a given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> List of PasswordHistoryDto </returns>
    public Task<List<PasswordHistoryDto>> GetPasswordHistoryDtos(string userId);

    /// <summary>
    ///     Gets all password histories for a given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> List of PasswordHistory </returns>
    public Task<List<PasswordHistory>> GetPasswordHistories(string userId);
    
    /// <summary>
    ///     Updates a password history.
    /// </summary>
    /// <param name="passwordHistory"></param>
    /// <returns></returns>
    public Task<IdentityResult> UpdateAsync(PasswordHistory passwordHistory);
}