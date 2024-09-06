using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface IStepConditionService
{
    Task<IdentityResult> CreateAsync(WkfProcessStepCondition stepCondition);

    Task<IdentityResult> CreateAsync(IEnumerable<WkfProcessStepCondition> stepConditions);

    Task<IdentityResult> UpdateAsync(WkfProcessStepCondition stepCondition);

    Task<IdentityResult> DeleteAsync(WkfProcessStepCondition stepCondition);

    Task<WkfProcessStepCondition?> FindByIdAsync(string id);

    Task<IEnumerable<WkfProcessStepCondition>> FindByCurrentStepIdAsync(string currentStepId);

    Task<IEnumerable<WkfProcessStepCondition>> FindByNextStepIdAsync(string nextStepId);

    Task<IEnumerable<WkfProcessStepCondition>> FindByConditionAsync(string condition);
}