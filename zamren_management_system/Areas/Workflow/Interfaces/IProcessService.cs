using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface IProcessService
{
    Task<ICollection<WkfProcess>> FindAllAsync();
    Task<ICollection<WkfProcess>> FindByModuleCode(string moduleCode);
    Task<WkfProcess?> FindByIdAsync(string id);
    Task<IdentityResult> CreateAsync(WkfProcess process);
    Task<IdentityResult> UpdateAsync(WkfProcess process);
    Task<IdentityResult> DeleteAsync(WkfProcess process);
    Task<WkfProcess?> FindByNameAsync(string name);
    Task<bool> ProcessNameExistsExceptAsync(string name, string processId);
    Task<ICollection<WkfProcessStep>> FindStepsAsync(string processId);
    Task<int> CountStepsAsync(string processId);

}