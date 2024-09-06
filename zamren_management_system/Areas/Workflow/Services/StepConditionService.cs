using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class StepConditionService : IStepConditionService
{
    private readonly AuthContext _context;
    private readonly ILogger<StepConditionService> _logger;

    public StepConditionService(AuthContext context, ILogger<StepConditionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(WkfProcessStepCondition stepCondition)
    {
        try
        {
            await _context.WkfProcessStepConditions.AddAsync(stepCondition);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateStepConditionAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> CreateAsync(IEnumerable<WkfProcessStepCondition> stepConditions)
    {
        try
        {
            _context.WkfProcessStepConditions.AddRange(stepConditions);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateStepConditionAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(WkfProcessStepCondition stepCondition)
    {
        try
        {
            _context.WkfProcessStepConditions.Update(stepCondition);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateStepConditionAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(WkfProcessStepCondition stepCondition)
    {
        try
        {
            _context.WkfProcessStepConditions.Remove(stepCondition);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteStepConditionAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<WkfProcessStepCondition?> FindByIdAsync(string id)
    {
        return await _context.WkfProcessStepConditions
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<WkfProcessStepCondition>> FindByCurrentStepIdAsync(string currentStepId)
    {
        return await _context.WkfProcessStepConditions
            .Where(t => t.CurrentStepId == currentStepId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WkfProcessStepCondition>> FindByNextStepIdAsync(string nextStepId)
    {
        return await _context.WkfProcessStepConditions
            .Where(t => t.NextStepId == nextStepId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WkfProcessStepCondition>> FindByConditionAsync(string condition)
    {
        return await _context.WkfProcessStepConditions
            .Where(t => t.Condition == condition)
            .ToListAsync();
    }
}