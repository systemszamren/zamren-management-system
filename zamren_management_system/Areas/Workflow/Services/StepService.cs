using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Responses;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class StepService : IStepService
{
    private readonly AuthContext _context;
    private readonly ILogger<StepService> _logger;
    private readonly IRoleService _roleService;
    private readonly IOrganizationService _organizationService;
    private readonly IBranchService _branchService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentService _departmentService;
    private readonly IOfficeService _officeService;
    private readonly ITaskService _taskService;

    public StepService(AuthContext context, ILogger<StepService> logger,
        IOrganizationService organizationService,
        IBranchService branchService, IDepartmentService departmentService,
        IOfficeService officeService, IRoleService roleService, UserManager<ApplicationUser> userManager,
        ITaskService taskService)
    {
        _context = context;
        _logger = logger;
        _organizationService = organizationService;
        _branchService = branchService;
        _departmentService = departmentService;
        _officeService = officeService;
        _roleService = roleService;
        _userManager = userManager;
        _taskService = taskService;
    }

    public async Task<IdentityResult> CreateAsync(WkfProcessStep step)
    {
        try
        {
            await _context.WkfProcessSteps.AddAsync(step);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(WkfProcessStep step)
    {
        try
        {
            _context.WkfProcessSteps.Update(step);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(WkfProcessStep step)
    {
        try
        {
            _context.WkfProcessSteps.Remove(step);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<WkfProcessStep?> FindByIdAsync(string id)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Privilege)
            .Include(s => s.Process)
            .ThenInclude(p => p.Module)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    //FindByNameAsync
    public async Task<WkfProcessStep?> FindByNameAsync(string name)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Process)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<List<WkfProcessStep>> FindByProcessIdAsync(string processId)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Process)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .Where(s => s.ProcessId == processId)
            .ToListAsync();
    }

    public async Task<WkfProcessStep?> FindInitialStepAsync(string processId)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Process)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.ProcessId == processId && s.IsInitialStep);
    }

    public async Task<WkfProcessStep?> FindFinalStepAsync(string processId)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Process)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.ProcessId == processId && s.IsFinalStep);
    }

    //IsNextStepAsync
    public async Task<bool> IsNextStepAsync(string stepId)
    {
        return await _context.WkfProcessSteps
            .AnyAsync(s => s.NextStepId == stepId);
    }

    //IsPreviousStepAsync
    public async Task<bool> IsPreviousStepAsync(string stepId)
    {
        return await _context.WkfProcessSteps
            .AnyAsync(s => s.PreviousStepId == stepId);
    }

    public async Task<(List<WkfProcessStep> steps, CustomIdentityResult response)> GetOrderedStepsAsync(
        string processId)
    {
        //get all steps belonging to the process
        var allSteps = await FindByProcessIdAsync(processId);
        if (allSteps.Count == 0)
            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "No steps found for this process. Kindly add steps to the process"
            }));

        //loop through the steps to find the initial step
        var initialSteps = allSteps.FindAll(s => s.IsInitialStep);
        if (initialSteps.Count > 1)
        {
            //number all steps
            NumberingUnorderedSteps(allSteps);

            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "Multiple initial steps found for this process. A process can only have one initial step"
            }));
        }

        if (!initialSteps.Any())
        {
            //number all steps
            NumberingUnorderedSteps(allSteps);

            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "No initial step found for this process. A process must have an initial step"
            }));
        }

        if (initialSteps.Count == 1)
        {
            //check if the initial step has a next step
            if (initialSteps[0].NextStepId == null)
            {
                //number all steps
                NumberingUnorderedSteps(allSteps);

                return (allSteps, CustomIdentityResult.Failed(new IdentityError
                {
                    Description = "The initial step does not have a next step configured. Kindly add a next step"
                }));
            }

            initialSteps[0].Order = 1;
            initialSteps[0].Ordered = true;
        }
        else if (initialSteps.Count > 1)
        {
            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "Multiple initial steps found for this process. A process can only have one initial step"
            }));
        }

        //loop through the steps to find the final step
        var finalSteps = allSteps.FindAll(s => s.IsFinalStep);
        if (finalSteps.Count > 1)
        {
            //number all steps
            NumberingUnorderedSteps(allSteps);

            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "Multiple final steps found for this process. A process can only have one final step"
            }));
        }

        if (!finalSteps.Any())
        {
            //number all steps
            NumberingUnorderedSteps(allSteps);

            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "No final step found for this process. A process must have a final step"
            }));
        }

        //loop through the steps to find the next steps
        var currentStep = initialSteps[0];
        while (allSteps.Any(s => s.PreviousStepId == currentStep.Id))
        {
            currentStep = allSteps.FirstOrDefault(s => s.PreviousStepId == currentStep.Id); //magic

            if (currentStep == null) break;

            currentStep.Order = allSteps.Max(s => s.Order) + 1;
            currentStep.Ordered = true;
        }

        //number all steps
        NumberingUnorderedSteps(allSteps);


        //Order the final step
        var finalStep = finalSteps[0];

        //check if the final step has a previous step
        if (finalStep.PreviousStepId == null)
        {
            //number all steps
            NumberingUnorderedSteps(allSteps);

            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "The final step does not have a previous step configured. Kindly add a previous step"
            }));
        }

        finalStep.Order = allSteps.Count;
        finalStep.Ordered = true;

        //verify if all steps have a previous and next step configured correctly
        return VerifyStepConfiguration(allSteps);
    }

    //loop and check if all steps have a previous and next step configured. Only initial step should not have a previous step and only final step should not have a next step. Only one initial step and one final step should exist
    //return async Task<(List<WkfProcessStep> steps, IdentityResult response)>
    private static (List<WkfProcessStep> steps, CustomIdentityResult response) VerifyStepConfiguration(
        List<WkfProcessStep> allSteps)
    {
        var initialSteps = allSteps.FindAll(s => s.IsInitialStep);
        if (initialSteps.Count != 1)
            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "Multiple initial steps found for this process. A process can only have one initial step"
            }));

        var finalSteps = allSteps.FindAll(s => s.IsFinalStep);
        if (finalSteps.Count != 1)
            return (allSteps, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "Multiple final steps found for this process. A process can only have one final step"
            }));

        foreach (var step in allSteps)
        {
            switch (step)
            {
                case { IsInitialStep: true, PreviousStepId: not null }:
                    return (allSteps, CustomIdentityResult.Failed(new IdentityError
                    {
                        Description = "The first step should not have a previous step"
                    }));
                case { IsFinalStep: true, NextStepId: not null }:
                    return (allSteps, CustomIdentityResult.Failed(new IdentityError
                    {
                        Description = "The final step should not have a next step"
                    }));
                case { IsInitialStep: false, PreviousStepId: null }:
                    return (allSteps, CustomIdentityResult.Failed(new IdentityError
                    {
                        Description = "Step (" + step.Name + ") should have a previous step"
                    }));
                case { IsFinalStep: false, NextStepId: null }:
                    return (allSteps, CustomIdentityResult.Failed(new IdentityError
                    {
                        Description = "Step (" + step.Name + ") should have a next step"
                    }));
            }
        }

        return (allSteps, CustomIdentityResult.Success("All steps have been ordered successfully"));
    }

    private static void NumberingUnorderedSteps(List<WkfProcessStep> allSteps)
    {
        var unorderedSteps = allSteps.Where(s => s is { Order: 0, IsFinalStep: false }).ToList();
        if (!unorderedSteps.Any()) return;
        foreach (var step in unorderedSteps)
            step.Order = allSteps.Max(s => s.Order) + 1;
    }

    //FindAllByProcessIdAsync
    public async Task<ICollection<WkfProcessStep>> FindAllByProcessIdAsync(string processId)
    {
        try
        {
            return await _context.WkfProcessSteps
                .Where(s => s.ProcessId == processId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<WkfProcessStep>();
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindEmployeesNotInStepAsync(WkfProcessStep step)
    {
        try
        {
            if (step.ActioningUser != null)
                return await _context.Users
                    .Where(u => u.Id != step.ActioningUserId && u.IsEmployee && u.CanActionWkfTasks)
                    .ToListAsync();

            return await _context.Users
                .Where(u => u.IsEmployee && u.CanActionWkfTasks)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }

    //FindAllByProcessIdExceptAsync
    public async Task<ICollection<WkfProcessStep>> FindAllByProcessIdExceptAsync(string processId, string stepId)
    {
        try
        {
            return await _context.WkfProcessSteps
                .Where(s => s.ProcessId == processId && s.Id != stepId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<WkfProcessStep>();
        }
    }

    public async Task<(IEnumerable<ApplicationUser> Users, CustomIdentityResult response)> FindActioningUsersAsync(
        WkfProcessStep step)
    {
        var users = new List<ApplicationUser>();
        
        //flag to check if step can be actioned by a user with a specific role
        var searchRole = false;
        
        //flag to check if step can be actioned by a user in a specific organizational unit
        var searchOrganization = false;
        
        //flag to check if step can be actioned by a user with a specific role that has a specific privilege
        var searchPrivilege = false;
        
        //dynamic response text
        var responseText = "";

        try
        {
            switch (step)
            {
                //check if step is auto approved
                case { IsAutoApproved: true }:
                    responseText =
                        "This step is marked as Auto Approval meaning it will NOT require any users to action it.";
                    users = new List<ApplicationUser>();
                    break;

                //check if step has specific actioning user
                case { IsAutoApproved: false, ActioningUser: not null }:
                    responseText = $"This step can only be assigned to one user ({step.ActioningUser.FullName}).";
                    users.Add(step.ActioningUser);
                    break;

                //check if step has role and/or belongs to an organizational unit
                case { IsAutoApproved: false, ActioningUser: null }:
                {
                    var userWithRoles = new List<ApplicationUser>();

                    //check if step has role and privilege
                    if (!string.IsNullOrEmpty(step.RoleId) && step.Role != null)
                    {
                        searchRole = true;

                        if (!string.IsNullOrEmpty(step.PrivilegeId) && step.Privilege != null)
                        {
                            searchPrivilege = true;

                            //if role has privilege, get users with role and privilege
                            var roleHasPrivilege = await _roleService.HasPrivilegeAsync(step.RoleId, step.PrivilegeId);
                            if (roleHasPrivilege)
                                userWithRoles =
                                    (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync(step.Role.Name!);
                        }
                        else
                        {
                            //if role does not have privilege, get users with role only
                            userWithRoles =
                                (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync(step.Role.Name!);
                        }
                    }

                    var usersInOrganization = new List<ApplicationUser>();
                    
                    //check if organization is set
                    if (step.OrganizationId != null)
                    {
                        searchOrganization = true;
                        
                        //check if branch, department or office is set
                        if (step.BranchId != null)
                        {
                            if (step.DepartmentId != null)
                            {
                                if (step.OfficeId != null)
                                {
                                    //get users in office
                                    var usersInOffice = await _officeService.FindUsersAsync(step.OfficeId);
                                    usersInOrganization.AddRange(usersInOffice.Select(uo => uo.User));
                                }
                                else
                                {
                                    //get users in department and organization if office is not set
                                    usersInOrganization =
                                        (List<ApplicationUser>)await _departmentService.FindUsersAsync(
                                            step.DepartmentId);
                                }
                            }
                            else
                            {
                                //get users in branch and organization if department is not set
                                usersInOrganization =
                                    (List<ApplicationUser>)await _branchService.FindUsersAsync(step.BranchId);
                            }
                        }
                        else
                        {
                            //get users in organization if branch is not set
                            usersInOrganization =
                                (List<ApplicationUser>)await _organizationService.FindUsersAsync(step.OrganizationId);
                        }
                    }

                    // Dynamically generate response text based on conditions
                    // check if step is assigned to users with specific role and organization
                    if (searchRole && searchOrganization)
                    {
                        responseText =
                            $"This Step is assigned to users that belong to '{step.Organization?.Name}' Organization";
                        if (step.BranchId != null) responseText += $", '{step.Branch?.Name}' Branch";
                        if (step.DepartmentId != null) responseText += $", '{step.Department?.Name}' Department";
                        if (step.OfficeId != null) responseText += $", '{step.Office?.Name}' Office";
                        responseText += $" and have '{step.Role?.Name}' Role";
                        if (searchPrivilege)
                            responseText += $". This role should have '{step.Privilege?.Name}' Privilege";
                        responseText += ".";
                        users = userWithRoles.Intersect(usersInOrganization).ToList();
                    }
                    else if (searchRole && !searchOrganization) //if role is set but organization is not set
                    {
                        responseText = $"This step is assigned to users that have '{step.Role?.Name}' Role";
                        if (searchPrivilege)
                            responseText += $". This role should have '{step.Privilege?.Name}' Privilege";
                        responseText += ".";
                        users = userWithRoles;
                    }
                    else if (!searchRole && searchOrganization)
                    {
                        responseText =
                            $"This step is assigned to users that belong to '{step.Organization?.Name}' Organization";
                        if (step.BranchId != null) responseText += $", '{step.Branch?.Name}' Branch";
                        if (step.DepartmentId != null) responseText += $", '{step.Department?.Name}' Department";
                        if (step.OfficeId != null) responseText += $", '{step.Office?.Name}' Office";
                        responseText += ".";
                        users = usersInOrganization;
                    }
                    else
                    {
                        responseText = "This step does not have specific role or organization requirements.";
                        users = new List<ApplicationUser>();
                    }

                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            responseText = "An error occurred while processing the request.";
        }

        var usersThatCanActionStep = users
            .Where(u => u is { IsEmployee: true, CanActionWkfTasks: true })
            .ToList();

        //add text to response where user is marked as employee and can action wfk tasks
        responseText +=
            $"\nRemember to mark the user as an employee and allow them to action workflow tasks from the user's profile.";

        return (usersThatCanActionStep, CustomIdentityResult.Success(responseText));
    }

    //FindActioningUserAsync
    public async Task<ApplicationUser?> FindActioningUserAsync(WkfProcessStep step)
    {
        var output = await FindActioningUsersAsync(step);

        if (!output.Users.Any()) return null;

        //check task table and pick user with the fewest tasks if multiple users have the same number of tasks, pick at random
        var users = output.Users.ToList();
        //var int count, ApplicationUser user
        List<(int count, ApplicationUser user)> userWithTasks = new();

        foreach (var user in users)
        {
            var userTasks = await _taskService.FindByUserIdAsync(user.Id);
            userWithTasks.Add((userTasks.Count, user));
        }

        //sort users by number of tasks in ascending order
        userWithTasks.Sort((a, b) => a.count.CompareTo(b.count));

        //check if there are multiple users with the same least number of tasks
        var leastTasks = userWithTasks[0].count;
        var usersWithLeastTasks = userWithTasks.FindAll(u => u.count == leastTasks);

        if (usersWithLeastTasks.Count == 1)
            return usersWithLeastTasks[0].user;

        //if there are multiple users with the same least number of tasks, pick one at random
        var random = new Random();
        var index = random.Next(usersWithLeastTasks.Count);
        return usersWithLeastTasks[index].user;
    }

    //GetFirstStepAsync
    public async Task<WkfProcessStep?> GetFirstStepAsync(string processId)
    {
        return await _context.WkfProcessSteps
            .Include(s => s.Role)
            .Include(s => s.Privilege)
            .Include(s => s.Process)
            .ThenInclude(p => p.Module)
            .Include(s => s.ActioningUser)
            .Include(s => s.Office)
            .Include(s => s.Department)
            .Include(s => s.Branch)
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.ProcessId == processId && s.IsInitialStep);
    }
}