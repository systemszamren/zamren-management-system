using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface ITaskLogService
{
    Task<IdentityResult> CreateAsync(WkfTaskLog taskLog);

    Task<IdentityResult> UpdateAsync(WkfTaskLog taskLog);

    Task<IdentityResult> DeleteAsync(WkfTaskLog taskLog);

    Task<List<WkfTaskLog>> FindByTaskIdAsync(string taskId);
    
    Task<List<WkfTaskLog>> FindByTaskIdAndStepIdAsync(string taskId, string stepId);
}