using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface ITaskCommentService
{
    Task<IdentityResult> CreateAsync(WkfTaskComment taskComment);
    
    Task<IdentityResult> UpdateAsync(WkfTaskComment taskComment);
    
    Task<IdentityResult> DeleteAsync(WkfTaskComment taskComment);
}