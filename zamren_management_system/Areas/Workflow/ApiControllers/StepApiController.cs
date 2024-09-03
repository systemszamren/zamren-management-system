using zamren_management_system.Areas.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;
using zamren_management_system.Areas.Workflow.Dto;
using zamren_management_system.Areas.Workflow.Enums;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Area("Workflow")]
[Route("api/workflow/step")]
public class StepApiController : ControllerBase
{
    private readonly ILogger<StepApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProcessService _processService;
    private readonly IStepService _stepService;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ICypherService _cypherService;
    private readonly IDatatableService _datatableService;
    private readonly IOfficeService _officeService;
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly IOrganizationService _organizationService;
    private readonly IUserService _userService;
    private readonly IPrivilegeService _privilegeService;

    public StepApiController(ILogger<StepApiController> logger, UserManager<ApplicationUser> userManager,
        ICypherService cypherService, IProcessService processService,
        IStepService stepService, RoleManager<ApplicationRole> roleManager, IOfficeService officeService,
        IBranchService branchService, IDepartmentService departmentService, IOrganizationService organizationService,
        IDatatableService datatableService, IUserService userService, IPrivilegeService privilegeService)
    {
        _logger = logger;
        _userManager = userManager;
        _cypherService = cypherService;
        _processService = processService;
        _stepService = stepService;
        _roleManager = roleManager;
        _officeService = officeService;
        _branchService = branchService;
        _departmentService = departmentService;
        _organizationService = organizationService;
        _datatableService = datatableService;
        _userService = userService;
        _privilegeService = privilegeService;
    }

    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> CreateStep()
    {
        try
        {
            var stepDto = new WkfProcessStepDto
            {
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                Privilege = new PrivilegeDto
                {
                    Id = Request.Form["privilegeId"].FirstOrDefault()!
                },
                Process = new WkfProcessDto
                {
                    Id = Request.Form["processId"].FirstOrDefault()!
                },
                IsInitialStepString = Request.Form["isInitialStep"].FirstOrDefault(),
                IsFinalStepString = Request.Form["isFinalStep"].FirstOrDefault(),
                IsDepartmentHeadApprovedString = Request.Form["isDepartmentHeadApproved"].FirstOrDefault(),
                // IsAutoApprovedString = Request.Form["isAutoApproved"].FirstOrDefault(),
                IsAutoApprovedString = "false",
                RequestMap = Request.Form["requestMap"].FirstOrDefault(),
                SlaHours = int.TryParse(Request.Form["slaHours"].FirstOrDefault(), out var slaHours) ? slaHours : null,
                Role = new RoleDto
                {
                    Id = Request.Form["roleId"].FirstOrDefault()!
                },
                ActioningUser = new UserDto
                {
                    Id = Request.Form["actioningUserId"].FirstOrDefault()!
                },
                Office = new OfficeDto
                {
                    Id = Request.Form["officeId"].FirstOrDefault()!
                },
                Department = new DepartmentDto
                {
                    Id = Request.Form["departmentId"].FirstOrDefault()!
                },
                Branch = new BranchDto
                {
                    Id = Request.Form["branchId"].FirstOrDefault()!
                },
                Organization = new OrganizationDto
                {
                    Id = Request.Form["organizationId"].FirstOrDefault()!
                },
                PreviousStepId = Request.Form["previousStepId"].FirstOrDefault(),
                NextStepId = Request.Form["nextStepId"].FirstOrDefault(),
                // NextProcessId = Request.Form["nextProcessId"].FirstOrDefault(),
                // PrevProcessId = Request.Form["prevProcessId"].FirstOrDefault()
            };

            //DepartmentHeadApprovalString
            if (string.IsNullOrEmpty(stepDto.IsDepartmentHeadApprovedString))
                stepDto.IsDepartmentHeadApproved = false;
            else if (stepDto.IsDepartmentHeadApprovedString is "on" or "true")
                stepDto.IsDepartmentHeadApproved = true;
            else
                stepDto.IsDepartmentHeadApproved = false;

            //IsAutoApprovedString
            if (string.IsNullOrEmpty(stepDto.IsAutoApprovedString))
                stepDto.IsAutoApproved = false;
            else if (stepDto.IsAutoApprovedString is "on" or "true")
                stepDto.IsAutoApproved = true;
            else
                stepDto.IsAutoApproved = false;

            //IsInitialStepString
            if (string.IsNullOrEmpty(stepDto.IsInitialStepString))
                stepDto.IsInitialStep = false;
            else if (stepDto.IsInitialStepString is "on" or "true")
                stepDto.IsInitialStep = true;
            else
                stepDto.IsInitialStep = false;

            //IsFinalStepString
            if (string.IsNullOrEmpty(stepDto.IsFinalStepString))
                stepDto.IsFinalStep = false;
            else if (stepDto.IsFinalStepString is "on" or "true")
                stepDto.IsFinalStep = true;
            else
                stepDto.IsFinalStep = false;

            if (string.IsNullOrEmpty(stepDto.Name))
                return Ok(new { success = false, message = "Step name is required" });

            if (string.IsNullOrEmpty(stepDto.Description))
                return Ok(new { success = false, message = "Step description is required" });

            //privilege
            if (stepDto.IsAutoApproved == false)
                if (string.IsNullOrEmpty(stepDto.Privilege.Id))
                    return Ok(new { success = false, message = "Privilege is required" });

            //process
            if (string.IsNullOrEmpty(stepDto.Process.Id))
                return Ok(new { success = false, message = "Process is required" });

            stepDto.Process.Id = _cypherService.Decrypt(stepDto.Process.Id);
            if (await _processService.FindByIdAsync(stepDto.Process.Id) == null)
                return Ok(new { success = false, message = "Process not found" });

            //PrivilegeId
            if (!string.IsNullOrEmpty(stepDto.Privilege.Id))
            {
                stepDto.Privilege.Id = _cypherService.Decrypt(stepDto.Privilege.Id);
                if (await _privilegeService.FindByIdAsync(stepDto.Privilege.Id) == null)
                    return Ok(new { success = false, message = "Privilege not found" });
            }
            else
            {
                stepDto.Privilege.Id = null;
            }

            // PreviousStepId
            if (!string.IsNullOrEmpty(stepDto.PreviousStepId))
            {
                stepDto.PreviousStepId = _cypherService.Decrypt(stepDto.PreviousStepId);
                if (await _stepService.FindByIdAsync(stepDto.PreviousStepId) == null)
                    return Ok(new { success = false, message = "Previous step not found" });
            }
            else
            {
                stepDto.PreviousStepId = null;
            }

            // NextStepId
            if (!string.IsNullOrEmpty(stepDto.NextStepId))
            {
                stepDto.NextStepId = _cypherService.Decrypt(stepDto.NextStepId);
                if (await _stepService.FindByIdAsync(stepDto.NextStepId) == null)
                    return Ok(new { success = false, message = "Next step not found" });
            }
            else
            {
                stepDto.NextStepId = null;
            }

            // NextProcessId
            /*if (!string.IsNullOrEmpty(stepDto.NextProcessId))
            {
                stepDto.NextProcessId = _cypherService.Decrypt(stepDto.NextProcessId);
                if (await _processService.FindByIdAsync(stepDto.NextProcessId) == null)
                    return Ok(new { success = false, message = "Next process not found" });
            }
            else
            {
                stepDto.NextProcessId = null;
            }*/

            //PrevProcessId
            /*if (!string.IsNullOrEmpty(stepDto.PrevProcessId))
            {
                stepDto.PrevProcessId = _cypherService.Decrypt(stepDto.PrevProcessId);
                if (await _processService.FindByIdAsync(stepDto.PrevProcessId) == null)
                    return Ok(new { success = false, message = "Previous process not found" });
            }
            else
            {
                stepDto.PrevProcessId = null;
            }*/

            var taskIsAssignable = false;

            //actioning user
            if (!string.IsNullOrEmpty(stepDto.ActioningUser.Id) && stepDto.IsAutoApproved == false)
            {
                stepDto.ActioningUser.Id = _cypherService.Decrypt(stepDto.ActioningUser.Id);
                if (await _userManager.FindByIdAsync(stepDto.ActioningUser.Id) == null)
                    return Ok(new { success = false, message = "Actioning user not found" });

                //if actioning user is set and the step is auto approved, return an error
                if ((bool)stepDto.IsAutoApproved)
                    return Ok(new
                    {
                        success = false, message = "If the step is auto approved, the actioning user should not be set"
                    });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.ActioningUser.Id = null;
            }

            //role
            if (!string.IsNullOrEmpty(stepDto.Role.Id))
            {
                stepDto.Role.Id = _cypherService.Decrypt(stepDto.Role.Id);
                if (await _roleManager.FindByIdAsync(stepDto.Role.Id) == null)
                    return Ok(new { success = false, message = "Role not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Role.Id = null;
            }

            //office
            if (!string.IsNullOrEmpty(stepDto.Office.Id))
            {
                stepDto.Office.Id = _cypherService.Decrypt(stepDto.Office.Id);
                if (await _officeService.FindByIdAsync(stepDto.Office.Id) == null)
                    return Ok(new { success = false, message = "Office not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Office.Id = null;
            }

            //department
            if (!string.IsNullOrEmpty(stepDto.Department.Id))
            {
                stepDto.Department.Id = _cypherService.Decrypt(stepDto.Department.Id);
                if (await _departmentService.FindByIdAsync(stepDto.Department.Id) == null)
                    return Ok(new { success = false, message = "Department not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Department.Id = null;
            }

            //branch
            if (!string.IsNullOrEmpty(stepDto.Branch.Id))
            {
                stepDto.Branch.Id = _cypherService.Decrypt(stepDto.Branch.Id);
                if (await _branchService.FindByIdAsync(stepDto.Branch.Id) == null)
                    return Ok(new { success = false, message = "Branch not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Branch.Id = null;
            }

            //organization
            if (!string.IsNullOrEmpty(stepDto.Organization.Id))
            {
                stepDto.Organization.Id = _cypherService.Decrypt(stepDto.Organization.Id);
                if (await _organizationService.FindByIdAsync(stepDto.Organization.Id) == null)
                    return Ok(new { success = false, message = "Organization not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Organization.Id = null;
            }

            if ((bool)stepDto.IsDepartmentHeadApproved)
                taskIsAssignable = true;

            //if the step is NOT auto approved
            if ((bool)!stepDto.IsAutoApproved)
            {
                if (!taskIsAssignable)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "This task cannot be assigned. Kindly select at least 'Assignment Parameter' in order for the system to assign the task correctly"
                    });
            }

            // if the step is an initial step, check if there is already an initial step in the process
            if ((bool)stepDto.IsInitialStep)
            {
                var initialStep = await _stepService.FindInitialStepAsync(stepDto.Process.Id);
                if (initialStep != null)
                {
                    //unset the existing initial step and set the new one
                    initialStep.IsInitialStep = false;
                    await _stepService.UpdateAsync(initialStep);
                }
            }

            //if the step is a final step, check if there is already a final step in the process
            if ((bool)stepDto.IsFinalStep)
            {
                var finalStep = await _stepService.FindFinalStepAsync(stepDto.Process.Id);
                if (finalStep != null)
                {
                    //unset the existing final step and set the new one
                    finalStep.IsFinalStep = false;
                    await _stepService.UpdateAsync(finalStep);
                }
            }

            // Create the new step
            var newStep = new WkfProcessStep
            {
                Name = stepDto.Name,
                Description = stepDto.Description,
                PrivilegeId = stepDto.Privilege.Id,
                ProcessId = stepDto.Process.Id,
                IsInitialStep = (bool)stepDto.IsInitialStep,
                IsFinalStep = (bool)stepDto.IsFinalStep,
                IsDepartmentHeadApproved = (bool)stepDto.IsDepartmentHeadApproved,
                IsAutoApproved = (bool)stepDto.IsAutoApproved,
                PreviousStepId = stepDto.PreviousStepId,
                NextStepId = stepDto.NextStepId,
                // NextProcessId = stepDto.NextProcessId,
                // PrevProcessId = stepDto.PrevProcessId,
                RequestMap = stepDto.RequestMap,
                SlaHours = stepDto.SlaHours,
                RoleId = stepDto.Role.Id,
                ActioningUserId = stepDto.ActioningUser.Id,
                OfficeId = stepDto.Office.Id,
                DepartmentId = stepDto.Department.Id,
                BranchId = stepDto.Branch.Id,
                OrganizationId = stepDto.Organization.Id,
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTimeOffset.UtcNow
            };

            var result = await _stepService.CreateAsync(newStep);

            return result.Succeeded
                ? Ok(new { success = true, message = "Step created successfully" })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> EditStep()
    {
        try
        {
            var stepDto = new WkfProcessStepDto
            {
                Id = Request.Form["stepId"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                Privilege = new PrivilegeDto
                {
                    Id = Request.Form["privilegeId"].FirstOrDefault()!
                },
                Process = new WkfProcessDto
                {
                    Id = Request.Form["processId"].FirstOrDefault()!
                },
                IsInitialStepString = Request.Form["isInitialStep"].FirstOrDefault(),
                IsFinalStepString = Request.Form["isFinalStep"].FirstOrDefault(),
                // IsAutoApprovedString = Request.Form["isAutoApproved"].FirstOrDefault(),
                IsAutoApprovedString = "false",
                RequestMap = Request.Form["requestMap"].FirstOrDefault(),
                SlaHours = int.TryParse(Request.Form["slaHours"].FirstOrDefault(), out var slaHours) ? slaHours : null,
                Role = new RoleDto
                {
                    Id = Request.Form["roleId"].FirstOrDefault()!
                },
                ActioningUser = new UserDto
                {
                    Id = Request.Form["actioningUserId"].FirstOrDefault()!
                },
                SelectedUser = new UserDto
                {
                    Id = Request.Form["selectedUserId"].FirstOrDefault()!
                },
                Office = new OfficeDto
                {
                    Id = Request.Form["officeId"].FirstOrDefault()!
                },
                Department = new DepartmentDto
                {
                    Id = Request.Form["departmentId"].FirstOrDefault()!
                },
                Branch = new BranchDto
                {
                    Id = Request.Form["branchId"].FirstOrDefault()!
                },
                Organization = new OrganizationDto
                {
                    Id = Request.Form["organizationId"].FirstOrDefault()!
                },
                PreviousStepId = Request.Form["previousStepId"].FirstOrDefault(),
                NextStepId = Request.Form["nextStepId"].FirstOrDefault(),
                // NextProcessId = Request.Form["nextProcessId"].FirstOrDefault(),
                // PrevProcessId = Request.Form["prevProcessId"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(stepDto.Id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            //IsAutoApprovedString
            if (string.IsNullOrEmpty(stepDto.IsAutoApprovedString))
                stepDto.IsAutoApproved = false;
            else if (stepDto.IsAutoApprovedString is "on" or "true")
                stepDto.IsAutoApproved = true;
            else
                stepDto.IsAutoApproved = false;


            //IsInitialStepString
            if (string.IsNullOrEmpty(stepDto.IsInitialStepString))
                stepDto.IsInitialStep = false;
            else if (stepDto.IsInitialStepString is "on" or "true")
                stepDto.IsInitialStep = true;
            else
                stepDto.IsInitialStep = false;

            //IsFinalStepString
            if (string.IsNullOrEmpty(stepDto.IsFinalStepString))
                stepDto.IsFinalStep = false;
            else if (stepDto.IsFinalStepString is "on" or "true")
                stepDto.IsFinalStep = true;
            else
                stepDto.IsFinalStep = false;

            if (string.IsNullOrEmpty(stepDto.Name))
                return Ok(new { success = false, message = "Step name is required" });

            if (string.IsNullOrEmpty(stepDto.Description))
                return Ok(new { success = false, message = "Step description is required" });

            //privilege
            if (stepDto.IsAutoApproved == false)
                if (string.IsNullOrEmpty(stepDto.Privilege.Id))
                    return Ok(new { success = false, message = "Privilege is required" });

            //process
            if (string.IsNullOrEmpty(stepDto.Process.Id))
                return Ok(new { success = false, message = "Process is required" });

            stepDto.Process.Id = _cypherService.Decrypt(stepDto.Process.Id);
            if (await _processService.FindByIdAsync(stepDto.Process.Id) == null)
                return Ok(new { success = false, message = "Process not found" });

            //PrivilegeId
            if (!string.IsNullOrEmpty(stepDto.Privilege.Id))
            {
                // stepDto.Privilege.Id = _cypherService.Decrypt(stepDto.Privilege.Id);
                if (await _privilegeService.FindByIdAsync(stepDto.Privilege.Id) == null)
                    return Ok(new { success = false, message = "Privilege not found" });
            }
            else
            {
                stepDto.Privilege.Id = null;
            }

            // PreviousStepId
            if (!string.IsNullOrEmpty(stepDto.PreviousStepId))
            {
                // stepDto.PreviousStepId = _cypherService.Decrypt(stepDto.PreviousStepId);
                if (await _stepService.FindByIdAsync(stepDto.PreviousStepId) == null)
                    return Ok(new { success = false, message = "Previous step not found" });
            }
            else
            {
                stepDto.PreviousStepId = null;
            }

            // NextStepId
            if (!string.IsNullOrEmpty(stepDto.NextStepId))
            {
                // stepDto.NextStepId = _cypherService.Decrypt(stepDto.NextStepId);
                if (await _stepService.FindByIdAsync(stepDto.NextStepId) == null)
                    return Ok(new { success = false, message = "Next step not found" });
            }
            else
            {
                stepDto.NextStepId = null;
            }

            // NextProcessId
            /*if (!string.IsNullOrEmpty(stepDto.NextProcessId))
            {
                // stepDto.NextProcessId = _cypherService.Decrypt(stepDto.NextProcessId);
                if (await _processService.FindByIdAsync(stepDto.NextProcessId) == null)
                    return Ok(new { success = false, message = "Next process not found" });
            }
            else
            {
                stepDto.NextProcessId = null;
            }*/

            //PrevProcessId
            /*if (!string.IsNullOrEmpty(stepDto.PrevProcessId))
            {
                // stepDto.PrevProcessId = _cypherService.Decrypt(stepDto.PrevProcessId);
                if (await _processService.FindByIdAsync(stepDto.PrevProcessId) == null)
                    return Ok(new { success = false, message = "Previous process not found" });
            }
            else
            {
                stepDto.PrevProcessId = null;
            }*/


            var taskIsAssignable = false;

            //actioning user
            if (!string.IsNullOrEmpty(stepDto.ActioningUser.Id) && stepDto.IsAutoApproved == false)
            {
                stepDto.ActioningUser.Id = _cypherService.Decrypt(stepDto.ActioningUser.Id);
                if (await _userManager.FindByIdAsync(stepDto.ActioningUser.Id) == null)
                    return Ok(new { success = false, message = "Actioning user not found" });

                //if actioning user is set and the step is auto approved, return an error
                if ((bool)stepDto.IsAutoApproved)
                    return Ok(new
                    {
                        success = false,
                        message = "If the step is auto approved, the actioning user should not be set"
                    });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.ActioningUser.Id = null;
            }

            //if actioning user is equal to the selected user, return an error
            if (!string.IsNullOrEmpty(stepDto.SelectedUser.Id) && string.IsNullOrEmpty(stepDto.ActioningUser.Id) &&
                stepDto.IsAutoApproved == false)
            {
                stepDto.SelectedUser.Id = _cypherService.Decrypt(stepDto.SelectedUser.Id);
                if (await _userManager.FindByIdAsync(stepDto.SelectedUser.Id) == null)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "An error occurred while processing the request. Kindly refresh the page and try again"
                    });

                stepDto.ActioningUser.Id = stepDto.SelectedUser.Id;
                taskIsAssignable = true;
            }

            //role
            if (!string.IsNullOrEmpty(stepDto.Role.Id) && stepDto.ActioningUser.Id == null &&
                stepDto.IsAutoApproved == false)
            {
                // stepDto.Role.Id = _cypherService.Decrypt(stepDto.Role.Id);
                if (await _roleManager.FindByIdAsync(stepDto.Role.Id) == null)
                    return Ok(new { success = false, message = "Role not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Role.Id = null;
            }

            //office
            if (!string.IsNullOrEmpty(stepDto.Office.Id) && stepDto.ActioningUser.Id == null &&
                stepDto.IsAutoApproved == false)
            {
                // stepDto.Office.Id = _cypherService.Decrypt(stepDto.Office.Id);
                if (await _officeService.FindByIdAsync(stepDto.Office.Id) == null)
                    return Ok(new { success = false, message = "Office not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Office.Id = null;
            }

            //department
            if (!string.IsNullOrEmpty(stepDto.Department.Id) && stepDto.ActioningUser.Id == null &&
                stepDto.IsAutoApproved == false)
            {
                // stepDto.Department.Id = _cypherService.Decrypt(stepDto.Department.Id);
                if (await _departmentService.FindByIdAsync(stepDto.Department.Id) == null)
                    return Ok(new { success = false, message = "Department not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Department.Id = null;
            }

            //branch
            if (!string.IsNullOrEmpty(stepDto.Branch.Id) && stepDto.ActioningUser.Id == null &&
                stepDto.IsAutoApproved == false)
            {
                // stepDto.Branch.Id = _cypherService.Decrypt(stepDto.Branch.Id);
                if (await _branchService.FindByIdAsync(stepDto.Branch.Id) == null)
                    return Ok(new { success = false, message = "Branch not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Branch.Id = null;
            }

            //organization
            if (!string.IsNullOrEmpty(stepDto.Organization.Id) && stepDto.ActioningUser.Id == null &&
                stepDto.IsAutoApproved == false)
            {
                // stepDto.Organization.Id = _cypherService.Decrypt(stepDto.Organization.Id);
                if (await _organizationService.FindByIdAsync(stepDto.Organization.Id) == null)
                    return Ok(new { success = false, message = "Organization not found" });

                taskIsAssignable = true;
            }
            else
            {
                stepDto.Organization.Id = null;
            }

            //if the step is NOT auto approved
            if ((bool)!stepDto.IsAutoApproved)
            {
                if (!taskIsAssignable)
                    return Ok(new
                    {
                        success = false,
                        message =
                            "This task cannot be assigned. Kindly select at least 'Assignment Parameter' in order for the system to assign the task correctly"
                    });
            }

            // if the step is an initial step, check if there is already an initial step in the process
            if ((bool)stepDto.IsInitialStep)
            {
                var initialStep = await _stepService.FindInitialStepAsync(stepDto.Process.Id);
                if (initialStep != null)
                {
                    //unset existing initial step and set the new one
                    initialStep.IsInitialStep = false;
                    await _stepService.UpdateAsync(initialStep);
                    // return Ok(new { success = false, message = "Initial step already exists" });
                }
            }

            //if the step is a final step, check if there is already a final step in the process
            if ((bool)stepDto.IsFinalStep)
            {
                var finalStep = await _stepService.FindFinalStepAsync(stepDto.Process.Id);
                if (finalStep != null)
                {
                    //unset existing final step and set the new one
                    finalStep.IsFinalStep = false;
                    await _stepService.UpdateAsync(finalStep);
                    // return Ok(new { success = false, message = "Final step already exists" });
                }
            }

            // Find the existing step
            stepDto.Id = _cypherService.Decrypt(stepDto.Id);
            var existingStep = await _stepService.FindByIdAsync(stepDto.Id);
            if (existingStep == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            // Update the existing step
            existingStep.Name = stepDto.Name;
            existingStep.Description = stepDto.Description;
            existingStep.PrivilegeId = stepDto.Privilege.Id;
            existingStep.ProcessId = stepDto.Process.Id;
            existingStep.IsInitialStep = (bool)stepDto.IsInitialStep;
            existingStep.IsFinalStep = (bool)stepDto.IsFinalStep;
            existingStep.IsAutoApproved = (bool)stepDto.IsAutoApproved;
            existingStep.PreviousStepId = stepDto.PreviousStepId;
            existingStep.NextStepId = stepDto.NextStepId;
            // existingStep.NextProcessId = stepDto.NextProcessId;
            // existingStep.PrevProcessId = stepDto.PrevProcessId;
            existingStep.RequestMap = stepDto.RequestMap;
            existingStep.SlaHours = stepDto.SlaHours;
            existingStep.RoleId = stepDto.Role.Id;
            existingStep.ActioningUserId = stepDto.ActioningUser.Id;
            existingStep.OfficeId = stepDto.Office.Id;
            existingStep.DepartmentId = stepDto.Department.Id;
            existingStep.BranchId = stepDto.Branch.Id;
            existingStep.OrganizationId = stepDto.Organization.Id;
            existingStep.ModifiedByUserId = _userManager.GetUserId(User);
            existingStep.ModifiedDate = DateTimeOffset.UtcNow;

            var result = await _stepService.UpdateAsync(existingStep);

            return result.Succeeded
                ? Ok(new { success = true, message = "Step updated successfully" })
                : Ok(new { success = false, message = "An error occurred while processing the request" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> DeleteStep([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var step = await _stepService.FindByIdAsync(id);
            if (step == null)
                return Ok(new { success = false, message = "Step not found" });

            //check if the step is the next step in any other step
            var isNextStep = await _stepService.IsNextStepAsync(id);
            if (isNextStep)
                return Ok(new
                    { success = false, message = "Cannot delete step. It is set as a next step of another step" });

            //check if the step is the previous step in any other step
            var isPreviousStep = await _stepService.IsPreviousStepAsync(id);
            if (isPreviousStep)
                return Ok(new
                    { success = false, message = "Cannot delete step. It is set as a previous step of another step" });

            //delete step
            await _stepService.DeleteAsync(step);
            return Ok(new { success = true, message = "Step deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-steps-in-process-dt")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetOrderedSteps()
    {
        try
        {
            var processId = Request.Form["processId"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(processId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            processId = _cypherService.Decrypt(processId);
            var output = await _stepService.GetOrderedStepsAsync(processId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                output.steps.Select((step, _) => new WkfProcessStepDto
                {
                    Id = _cypherService.Encrypt(step.Id),
                    Name = step.Name,
                    Description = step.Description,
                    Privilege = new PrivilegeDto
                    {
                        Id = _cypherService.Encrypt(step.PrivilegeId),
                        Name = step.Privilege?.Name,
                        Description = step.Privilege?.Description
                    },
                    Ordered = step.Ordered.ToString(),
                    Order = step.Order,
                    Process = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(step.ProcessId),
                        Name = step.Process.Name,
                        Description = step.Process.Description
                    },
                    IsInitialStep = step.IsInitialStep,
                    IsFinalStep = step.IsFinalStep,
                    IsAutoApproved = step.IsAutoApproved,
                    /*NextProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(step.NextProcessId),
                        Name = step.NextProcess?.Name,
                        Description = step.NextProcess?.Description
                    },
                    PrevProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(step.PrevProcessId),
                        Name = step.PrevProcess?.Name,
                        Description = step.PrevProcess?.Description
                    },*/
                    NextStep = new WkfProcessStepDto
                    {
                        Id = _cypherService.Encrypt(step.NextStepId),
                        Name = step.NextStep?.Name,
                        Description = step.NextStep?.Description
                    },
                    PreviousStep = new WkfProcessStepDto
                    {
                        Id = _cypherService.Encrypt(step.PreviousStepId),
                        Name = step.PreviousStep?.Name,
                        Description = step.PreviousStep?.Description
                    },
                    RequestMap = step.RequestMap,
                    Role = new RoleDto
                    {
                        Id = _cypherService.Encrypt(step.RoleId),
                        Name = step.Role?.Name,
                        Description = step.Role?.Description
                    },
                    ActioningUser = new UserDto
                    {
                        Id = _cypherService.Encrypt(step.ActioningUserId),
                        FirstName = step.ActioningUser?.FirstName,
                        LastName = step.ActioningUser?.LastName,
                        Email = step.ActioningUser?.Email
                    },
                    Office = new OfficeDto
                    {
                        Id = _cypherService.Encrypt(step.OfficeId),
                        Name = step.Office?.Name,
                        Description = step.Office?.Description
                    },
                    Department = new DepartmentDto
                    {
                        Id = _cypherService.Encrypt(step.DepartmentId),
                        Name = step.Department?.Name,
                        Description = step.Department?.Description
                    },
                    Branch = new BranchDto
                    {
                        Id = _cypherService.Encrypt(step.BranchId),
                        Name = step.Branch?.Name,
                        Description = step.Branch?.Description
                    },
                    Organization = new OrganizationDto
                    {
                        Id = _cypherService.Encrypt(step.OrganizationId),
                        Name = step.Organization?.Name,
                        Description = step.Organization?.Description
                    }
                }).ToList(),
                output.response
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-process-data")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetProcessData()
    {
        try
        {
            var processId = Request.Form["processId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(processId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            processId = _cypherService.Decrypt(processId);
            var process = await _processService.FindByIdAsync(processId);
            if (process == null)
                return Ok(new { success = false, message = "Process not found" });

            return Ok(new
            {
                success = true,
                process = new WkfProcessDto
                {
                    Id = _cypherService.Encrypt(process.Id),
                    Name = process.Name,
                    Description = process.Description,
                    StartingStepId = process.StartingStepId,
                    Module = new ModuleDto
                    {
                        Id = _cypherService.Encrypt(process.ModuleId),
                        Name = process.Module.Name,
                        Code = process.Module.Code,
                        Description = process.Module.Description
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-organizations")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetOrganizations()
    {
        try
        {
            var organizations = await _organizationService.FindAllAsync();
            return Ok(new
            {
                success = true,
                organizations = organizations.Select(organization => new OrganizationDto
                {
                    Id = _cypherService.Encrypt(organization.Id),
                    Name = organization.Name,
                    Description = organization.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-branches")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetBranches()
    {
        try
        {
            var branches = await _branchService.FindAllAsync();
            return Ok(new
            {
                success = true,
                branches = branches.Select(branch => new BranchDto
                {
                    Id = _cypherService.Encrypt(branch.Id),
                    Name = branch.Name,
                    Description = branch.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-departments")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetDepartments()
    {
        try
        {
            var departments = await _departmentService.FindAllAsync();
            return Ok(new
            {
                success = true,
                departments = departments.Select(department => new DepartmentDto
                {
                    Id = _cypherService.Encrypt(department.Id),
                    Name = department.Name,
                    Description = department.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-offices")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetOffices()
    {
        try
        {
            var offices = await _officeService.FindAllAsync();
            return Ok(new
            {
                success = true,
                offices = offices.Select(office => new OfficeDto
                {
                    Id = _cypherService.Encrypt(office.Id),
                    Name = office.Name,
                    Description = office.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-roles")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(new
            {
                success = true,
                roles = roles.Select(role => new RoleDto
                {
                    Id = _cypherService.Encrypt(role.Id),
                    Name = role.Name,
                    Description = role.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-privileges
    [HttpPost("get-privileges")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetPrivileges()
    {
        try
        {
            var processId = Request.Form["processId"].FirstOrDefault() ?? "";
            processId = _cypherService.Decrypt(processId);
            var process = await _processService.FindByIdAsync(processId);
            if (process == null)
                return Ok(new { success = false, message = "Process not found" });

            var privileges = await _privilegeService.FindInModuleAsync(process.ModuleId);
            privileges = privileges.OrderBy(p => p.Name).ToList();

            return Ok(new
            {
                success = true,
                privileges = privileges.Select(privilege => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege.Id),
                    Name = privilege.Name,
                    Description = privilege.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-users
    [HttpPost("get-employees-select2")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetEmployeesSelect2()
    {
        try
        {
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;

            //get all users
            var allEmployeesAsync = await _userService.FindAllEmployeesAsync();

            // Filter the users based on the search term
            var usersQuery = allEmployeesAsync
                .Where(user =>
                    user.Email != null && (user.Email.Contains(q) || user.FirstName.Contains(q) ||
                                           user.LastName.Contains(q))).ToList();

            var totalCount = usersQuery.Count;

            var users = usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new UserDto
                {
                    Id = _cypherService.Encrypt(user.Id),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                })
                .ToList();

            return Ok(new { results = users, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-steps-in-process")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetStepsInProcess()
    {
        try
        {
            var processId = Request.Form["processId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(processId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            processId = _cypherService.Decrypt(processId);
            var steps = await _stepService.FindAllByProcessIdAsync(processId);

            return Ok(new
            {
                success = true,
                steps = steps.Select(step => new WkfProcessStepDto
                {
                    Id = _cypherService.Encrypt(step.Id),
                    Name = step.Name,
                    Description = step.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-processes-except")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetProcessesExcept()
    {
        try
        {
            var processId = Request.Form["processId"].FirstOrDefault() ?? "";
            var processes = await _processService.FindAllAsync();

            if (!string.IsNullOrEmpty(processId))
            {
                processId = _cypherService.Decrypt(processId);
                processes = processes.Where(p => p.Id != processId).ToList();
            }

            return Ok(new
            {
                success = true,
                processes = processes.Select(process => new WkfProcessDto
                {
                    Id = _cypherService.Encrypt(process.Id),
                    Name = process.Name,
                    Description = process.Description
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-step-data")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetStepData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var step = await _stepService.FindByIdAsync(id);
            if (step == null)
                return Ok(new { success = false, message = "Step not found" });

            List<Organization> organizations;
            var branches = new List<Branch>();
            var departments = new List<Department>();
            var offices = new List<Office>();

            //get organizations
            if (!string.IsNullOrEmpty(step.OrganizationId))
            {
                organizations = await _organizationService.FindAllAsync();

                //get branches in the organization
                if (!string.IsNullOrEmpty(step.BranchId))
                {
                    branches = (List<Branch>)await _branchService.FindByOrganizationIdAsync(step.OrganizationId);

                    //get departments in the branch
                    if (!string.IsNullOrEmpty(step.DepartmentId))
                    {
                        departments = (List<Department>)await _departmentService.FindByBranchIdAsync(step.BranchId);

                        //get offices in the department
                        if (!string.IsNullOrEmpty(step.OfficeId))
                        {
                            offices = await _officeService.FindByDepartmentIdAsync(step.DepartmentId);
                        }
                        else
                        {
                            offices = await _officeService.FindAllAsync();
                        }
                    }
                    else
                    {
                        departments = await _departmentService.FindAllAsync();
                    }
                }
                else
                {
                    branches = await _branchService.FindAllAsync();
                }
            }
            else
            {
                organizations = await _organizationService.FindAllAsync();
            }

            //get roles
            var roles = await _roleManager.Roles.ToListAsync();

            //get processes
            var processes = await _processService.FindAllAsync();

            //get steps except the current step
            // var steps = await _stepService.FindAllByProcessIdAsync(step.ProcessId);
            var steps = await _stepService.FindAllByProcessIdExceptAsync(step.ProcessId, step.Id);

            //get privileges
            var privileges = await _privilegeService.FindInModuleAsync(step.Process.ModuleId);

            //get sla hours from enum and convert to list
            var slaHoursList = Enum.GetValues(typeof(SlaHours))
                .Cast<SlaHours>()
                .Select(slaHours => new SlaHoursDto
                {
                    Hour = (int)slaHours,
                    Description = slaHours.GetDescription()
                })
                .ToList();

            return Ok(new
            {
                success = true,
                step = new WkfProcessStepDto
                {
                    Id = _cypherService.Encrypt(step.Id),
                    Name = step.Name,
                    Description = step.Description,
                    Privilege = new PrivilegeDto
                    {
                        Id = _cypherService.Encrypt(step.PrivilegeId),
                        PlainId = step.PrivilegeId,
                        Name = step.Privilege?.Name,
                        Description = step.Privilege?.Description
                    },
                    Ordered = step.Ordered.ToString(),
                    Order = step.Order,
                    SlaHours = step.SlaHours,
                    Process = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(step.ProcessId),
                        Name = step.Process.Name,
                        Description = step.Process.Description,
                        Module = new ModuleDto
                        {
                            Id = _cypherService.Encrypt(step.Process.ModuleId),
                            Name = step.Process.Module.Name,
                            Code = step.Process.Module.Code,
                            Description = step.Process.Module.Description
                        }
                    },
                    IsInitialStep = step.IsInitialStep,
                    IsFinalStep = step.IsFinalStep,
                    IsDepartmentHeadApproved = step.IsDepartmentHeadApproved,
                    IsAutoApproved = step.IsAutoApproved,
                    /*NextProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(step.NextProcessId),
                        PlainId = step.NextProcessId,
                        Name = step.NextProcess?.Name,
                        Description = step.NextProcess?.Description
                    },
                    PrevProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(step.PrevProcessId),
                        PlainId = step.PrevProcessId,
                        Name = step.PrevProcess?.Name,
                        Description = step.PrevProcess?.Description
                    },*/
                    NextStep = new WkfProcessStepDto
                    {
                        Id = _cypherService.Encrypt(step.NextStepId),
                        PlainId = step.NextStepId,
                        Name = step.NextStep?.Name,
                        Description = step.NextStep?.Description
                    },
                    PreviousStep = new WkfProcessStepDto
                    {
                        Id = _cypherService.Encrypt(step.PreviousStepId),
                        PlainId = step.PreviousStepId,
                        Name = step.PreviousStep?.Name,
                        Description = step.PreviousStep?.Description
                    },
                    RequestMap = step.RequestMap,
                    Role = new RoleDto
                    {
                        Id = _cypherService.Encrypt(step.RoleId),
                        PlainId = step.RoleId,
                        Name = step.Role?.Name,
                        Description = step.Role?.Description
                    },
                    ActioningUser = new UserDto
                    {
                        Id = _cypherService.Encrypt(step.ActioningUserId),
                        FirstName = step.ActioningUser?.FirstName,
                        LastName = step.ActioningUser?.LastName,
                        Email = step.ActioningUser?.Email
                    },
                    Office = new OfficeDto
                    {
                        Id = _cypherService.Encrypt(step.OfficeId),
                        PlainId = step.OfficeId,
                        Name = step.Office?.Name,
                        Description = step.Office?.Description
                    },
                    Department = new DepartmentDto
                    {
                        Id = _cypherService.Encrypt(step.DepartmentId),
                        PlainId = step.DepartmentId,
                        Name = step.Department?.Name,
                        Description = step.Department?.Description
                    },
                    Branch = new BranchDto
                    {
                        Id = _cypherService.Encrypt(step.BranchId),
                        PlainId = step.BranchId,
                        Name = step.Branch?.Name,
                        Description = step.Branch?.Description
                    },
                    Organization = new OrganizationDto
                    {
                        Id = _cypherService.Encrypt(step.OrganizationId),
                        PlainId = step.OrganizationId,
                        Name = step.Organization?.Name,
                        Description = step.Organization?.Description
                    },
                    Offices = offices.Select(office => new OfficeDto
                    {
                        Id = _cypherService.Encrypt(office.Id),
                        PlainId = office.Id,
                        Name = office.Name,
                        Description = office.Description
                    }).ToList(),
                    Departments = departments.Select(department => new DepartmentDto
                    {
                        Id = _cypherService.Encrypt(department.Id),
                        PlainId = department.Id,
                        Name = department.Name,
                        Description = department.Description
                    }).ToList(),
                    Branches = branches.Select(branch => new BranchDto
                    {
                        Id = _cypherService.Encrypt(branch.Id),
                        PlainId = branch.Id,
                        Name = branch.Name,
                        Description = branch.Description
                    }).ToList(),
                    Organizations = organizations.Select(organization => new OrganizationDto
                    {
                        Id = _cypherService.Encrypt(organization.Id),
                        PlainId = organization.Id,
                        Name = organization.Name,
                        Description = organization.Description
                    }).ToList(),
                    Roles = roles.Select(role => new RoleDto
                    {
                        Id = _cypherService.Encrypt(role.Id),
                        PlainId = role.Id,
                        Name = role.Name,
                        Description = role.Description
                    }).ToList(),
                    Processes = processes.Select(p => new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(p.Id),
                        PlainId = p.Id,
                        Name = p.Name,
                        Description = p.Description
                    }).ToList(),
                    Steps = steps.Select(s => new WkfProcessStepDto
                    {
                        Id = _cypherService.Encrypt(s.Id),
                        PlainId = s.Id,
                        Name = s.Name,
                        Description = s.Description
                    }).ToList(),
                    Privileges = privileges.Select(privilege => new PrivilegeDto
                    {
                        Id = privilege.Id,
                        PlainId = privilege.Id,
                        Name = privilege.Name,
                        Description = privilege.Description
                    }).ToList(),
                    SlaHoursList = slaHoursList
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-branches-by-organization")]
    public async Task<IActionResult> GetBranchesByOrganization()
    {
        try
        {
            var organizationId = Request.Form["organizationId"].FirstOrDefault() ?? "";
            var branches = await _branchService.FindByOrganizationIdAsync(organizationId);

            var branchesDto = branches.Select(branch => new BranchDto
            {
                PlainId = branch.Id,
                Name = branch.Name,
                Description = branch.Description
            }).ToList();

            return Ok(new { success = true, branches = branchesDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-departments-by-branch")]
    public async Task<IActionResult> GetDepartmentsByBranch()
    {
        try
        {
            var branchId = Request.Form["branchId"].FirstOrDefault() ?? "";
            var departments = await _departmentService.FindByBranchIdAsync(branchId);

            var departmentsDto = departments.Select(department => new DepartmentDto
            {
                PlainId = department.Id,
                Name = department.Name,
                Description = department.Description
            }).ToList();

            return Ok(new { success = true, departments = departmentsDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-offices-by-department")]
    public async Task<IActionResult> GetOfficesByDepartment()
    {
        try
        {
            var departmentId = Request.Form["departmentId"].FirstOrDefault() ?? "";
            var offices = await _officeService.FindByDepartmentIdAsync(departmentId);

            var officesDto = offices.Select(office => new OfficeDto
            {
                PlainId = office.Id,
                Name = office.Name,
                Description = office.Description
            }).ToList();

            return Ok(new { success = true, offices = officesDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-users-not-in-step-select2
    [HttpPost("get-employees-not-in-step-select2")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetEmployeesNotInStepSelect2()
    {
        try
        {
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;
            var stepId = Request.Form["stepId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(stepId)) return Ok(new { success = false, message = "Step not found" });

            stepId = _cypherService.Decrypt(stepId);
            var step = await _stepService.FindByIdAsync(stepId);
            if (step == null)
                return Ok(new { success = false, message = "Step not found" });

            //get all users not in the step by step id
            var usersNotInStep = await _stepService.FindEmployeesNotInStepAsync(step);

            // Filter the users based on the search term ignoring case
            var usersQuery = usersNotInStep
                .Where(user => user.Email != null && (user.FirstName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                                      user.LastName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                                      user.Email.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var totalCount = usersQuery.Count;

            var users = usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new UserDto
                {
                    Id = _cypherService.Encrypt(user.Id),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                })
                .ToList();

            return Ok(new { results = users, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-sla-hours")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public IActionResult GetSlaHours()
    {
        try
        {
            var slaHours = Enum.GetValues<SlaHours>().Select(slaHour => new
            {
                hour = (int)slaHour,
                description = slaHour.GetDescription()
            }).ToList();

            return Ok(new { success = true, slaHours });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-step-assignment-users-dt")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetStepAssignmentUsersDt()
    {
        try
        {
            var stepId = Request.Form["stepId"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(stepId)) return Ok(new { success = false, message = "Step not found" });

            stepId = _cypherService.Decrypt(stepId);
            var step = await _stepService.FindByIdAsync(stepId);
            if (step == null) return Ok(new { success = false, message = "Step not found" });

            var output = await _stepService.FindActioningUsersAsync(step);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Email!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                output.Users.ToList().Select((user, index) => new UserDto
                {
                    Counter = index + 1,
                    Id = _cypherService.Encrypt(user.Id),
                    FullName = user.FullName,
                    Email = user.Email,
                }).ToList(),
                output.response
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}