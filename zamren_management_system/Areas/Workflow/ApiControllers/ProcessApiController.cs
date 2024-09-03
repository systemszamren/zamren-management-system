using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Workflow.Dto;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Area("Workflow")]
[Route("api/workflow/process")]
public class ProcessApiController : ControllerBase
{
    private readonly IDatatableService _datatableService;
    private readonly ILogger<ProcessApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUtil _util;
    private readonly IProcessService _processService;
    private readonly ICypherService _cypherService;
    private readonly IModuleService _moduleService;

    public ProcessApiController(IDatatableService datatableService,
        ILogger<ProcessApiController> logger, UserManager<ApplicationUser> userManager, IUtil util,
        ICypherService cypherService, IProcessService processService, IModuleService moduleService)
    {
        _datatableService = datatableService;
        _logger = logger;
        _userManager = userManager;
        _util = util;
        _cypherService = cypherService;
        _processService = processService;
        _moduleService = moduleService;
    }

    [HttpPost("get-processes-dt")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetProcessesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var processes = await _processService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                processes.ToList().Select((process, index) => new WkfProcessDto
                {
                    Id = _cypherService.Encrypt(process.Id),
                    Name = process.Name,
                    Counter = index + 1,
                    Description = process.Description,
                    Module = new ModuleDto
                    {
                        Id = _cypherService.Encrypt(process.ModuleId),
                        Name = process.Module.Name,
                        Code = process.Module.Code,
                        Description = process.Module.Description
                    }
                }).ToList()
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
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var process = await _processService.FindByIdAsync(id);
            if (process == null)
                return Ok(new { success = false, message = "Process not found" });

            //get list of modules
            var modules = await _moduleService.FindAllAsync();

            return Ok(new
            {
                success = true,
                modules = modules.Select(module => new ModuleDto
                {
                    Id = _cypherService.Encrypt(module.Id),
                    PlainId = module.Id,
                    Name = module.Name,
                    Code = module.Code,
                    Description = module.Description
                }).ToList(),
                process = new WkfProcessDto
                {
                    Id = _cypherService.Encrypt(process.Id),
                    Name = process.Name,
                    Description = process.Description,
                    StartingStepId = process.StartingStepId,
                    Module = new ModuleDto
                    {
                        Id = _cypherService.Encrypt(process.ModuleId),
                        PlainId = process.Module.Id,
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

    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> CreateProcess()
    {
        try
        {
            var processDto = new WkfProcessDto
            {
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                StartingStepId = Request.Form["startingStepId"].FirstOrDefault()!,
                Module = new ModuleDto
                {
                    Id = Request.Form["moduleId"].FirstOrDefault()!
                }
            };

            if (string.IsNullOrEmpty(processDto.Description))
                return Ok(new { success = false, message = "Process description is required" });

            if (string.IsNullOrEmpty(processDto.Name))
                return Ok(new { success = false, message = "Process name is required" });

            if (string.IsNullOrEmpty(processDto.Module.Id))
                return Ok(new { success = false, message = "Module is required" });

            processDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(processDto.Name);

            processDto.Module.Id = _cypherService.Decrypt(processDto.Module.Id);
            if (await _moduleService.FindByIdAsync(processDto.Module.Id) == null)
            {
                return Ok(new { success = false, message = "Module not found" });
            }

            if (await _processService.FindByNameAsync(processDto.Name) != null)
            {
                return Ok(new { success = false, message = "Process already exists" });
            }

            var currentUserId = _userManager.GetUserId(User);

            await _processService.CreateAsync(new WkfProcess
            {
                Name = processDto.Name!,
                Description = processDto.Description,
                ModuleId = processDto.Module.Id,
                StartingStepId = processDto.StartingStepId,
                CreatedByUserId = currentUserId!,
                CreatedDate = DateTimeOffset.UtcNow
            });
            return Ok(new { success = true, message = "Process created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> EditProcess()
    {
        try
        {
            var processDto = new WkfProcessDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                Module = new ModuleDto
                {
                    Id = Request.Form["moduleId"].FirstOrDefault()!
                }
            };

            if (string.IsNullOrEmpty(processDto.Description))
                return Ok(new { success = false, message = "Process description is required" });

            if (string.IsNullOrEmpty(processDto.Name))
                return Ok(new { success = false, message = "Process name is required" });

            if (string.IsNullOrEmpty(processDto.Id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            processDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(processDto.Name);

            if (!string.IsNullOrEmpty(processDto.Module.Id))
            {
                processDto.Module.Id = _cypherService.Decrypt(processDto.Module.Id);
                if (await _moduleService.FindByIdAsync(processDto.Module.Id) == null)
                {
                    return Ok(new { success = false, message = "Module not found" });
                }
            }

            processDto.Id = _cypherService.Decrypt(processDto.Id!);
            if (await _processService.ProcessNameExistsExceptAsync(processDto.Name, processDto.Id))
            {
                return Ok(new { success = false, message = "Process with the same name already exists" });
            }

            var process = await _processService.FindByIdAsync(processDto.Id);
            if (process == null)
                return Ok(new { success = false, message = "Process not found" });

            process.ModuleId = !string.IsNullOrEmpty(processDto.Module.Id) ? processDto.Module.Id : process.ModuleId;

            process.Name = processDto.Name!;
            process.Description = processDto.Description;
            process.ModifiedByUserId = _userManager.GetUserId(User);
            process.ModifiedDate = DateTimeOffset.UtcNow;
            await _processService.UpdateAsync(process);
            return Ok(new { success = true, message = "Process updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> DeleteProcess([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var process = await _processService.FindByIdAsync(id);
            if (process == null)
                return Ok(new { success = false, message = "Process not found" });

            //check if the process has steps
            var steps = await _processService.FindStepsAsync(id);
            if (steps.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete process. It has " + steps.Count + " step(s)"
                });

            //delete process
            await _processService.DeleteAsync(process);
            return Ok(new { success = true, message = "Process deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-steps-in-process-select2")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetStepsInProcessSelect2()
    {
        try
        {
            var processId = Request.Form["processId"].FirstOrDefault() ?? "";
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;

            if (string.IsNullOrEmpty(processId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            processId = _cypherService.Decrypt(processId);
            var steps = await _processService.FindStepsAsync(processId);

            // Filter the steps based on the search term ignoring case
            var stepsQuery = steps
                .Where(step => step.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = stepsQuery.Count;

            var stepsList = stepsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(step => new WkfProcessStepDto
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
                });

            return Ok(new { results = stepsList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("count-steps-by-process-id")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> CountStepsByProcessId([FromForm] string processId)
    {
        try
        {
            if (string.IsNullOrEmpty(processId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            processId = _cypherService.Decrypt(processId);
            var process = await _processService.FindByIdAsync(processId);
            if (process == null)
                return Ok(new { success = false, message = "Process not found" });

            var totalCount = await _processService.CountStepsAsync(processId);
            return Ok(new { success = true, totalCount, processName = process.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-modules-select2")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetModulesSelect2()
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

            var modules = await _moduleService.FindAllAsync();

            // Filter the modules based on the search term ignoring case
            var modulesQuery = modules
                .Where(module => module.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                 module.Code.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = modulesQuery.Count;

            var modulesList = modulesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(module => new ModuleDto
                {
                    Id = _cypherService.Encrypt(module.Id),
                    Name = module.Name,
                    Code = module.Code,
                    Description = module.Description
                })
                .ToList();

            return Ok(new { results = modulesList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-modules-except")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetModulesExcept()
    {
        try
        {
            var moduleId = Request.Form["moduleId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            moduleId = _cypherService.Decrypt(moduleId);
            var modules = await _moduleService.FindAllExceptAsync(moduleId);

            return Ok(new
            {
                success = true,
                modules = modules.Select(module => new ModuleDto
                {
                    Id = _cypherService.Encrypt(module.Id),
                    Name = module.Name,
                    Code = module.Code,
                    Description = module.Description
                }).ToList()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}