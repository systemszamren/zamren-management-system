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
using zamren_management_system.Areas.Workflow.Enums;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Area("Workflow")]
[Route("api/workflow/task")]
public class TaskApiController : ControllerBase
{
    private readonly IDatatableService _datatableService;
    private readonly ILogger<TaskApiController> _logger;
    private readonly ICypherService _cypherService;
    private readonly ITaskService _taskService;
    private readonly ITaskLogService _taskLogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaskApiController(IDatatableService datatableService,
        ILogger<TaskApiController> logger,
        ICypherService cypherService, ITaskService taskService, UserManager<ApplicationUser> userManager,
        ITaskLogService taskLogService)
    {
        _datatableService = datatableService;
        _logger = logger;
        _cypherService = cypherService;
        _taskService = taskService;
        _userManager = userManager;
        _taskLogService = taskLogService;
    }

    [HttpPost("get-tasks-dt")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetTasksDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var tasks = await _taskService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                t => t.Reference!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.CurrentProcess!.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.CurrentStep!.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.TaskStartedByUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.CurrentActioningUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.PreviousActioningUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                t => t,
                tasks.ToList().Select((task, index) => new WkfTaskDto
                {
                    Id = _cypherService.Encrypt(task.Id),
                    Counter = index + 1,
                    Reference = task.Reference,
                    ParentTask = task.ParentTask != null
                        ? new WkfTaskDto
                        {
                            Id = _cypherService.Encrypt(task.ParentTask.Id),
                            Reference = task.ParentTask.Reference
                        }
                        : null,
                    CurrentProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(task.CurrentProcessId),
                        Name = task.CurrentProcess.Name,
                        Description = task.CurrentProcess.Description
                    },
                    CurrentProcessStartDate = task.CurrentProcessStartDate,
                    CurrentStep = task.CurrentStep != null
                        ? new WkfProcessStepDto
                        {
                            Id = _cypherService.Encrypt(task.CurrentStep.Id),
                            Name = task.CurrentStep.Name,
                            Description = task.CurrentStep.Description
                        }
                        : null,
                    TaskStartedByUser = new UserDto
                    {
                        Id = _cypherService.Encrypt(task.TaskStartedByUserId),
                        FirstName = task.TaskStartedByUser.FirstName,
                        LastName = task.TaskStartedByUser.LastName,
                        Email = task.TaskStartedByUser.Email
                    },
                    CurrentActioningUser = task.CurrentActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(task.CurrentActioningUserId),
                            FirstName = task.CurrentActioningUser.FirstName,
                            LastName = task.CurrentActioningUser.LastName,
                            Email = task.CurrentActioningUser.Email
                        }
                        : null,
                    PreviousActioningUser = task.PreviousActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(task.PreviousActioningUserId),
                            FirstName = task.PreviousActioningUser.FirstName,
                            LastName = task.PreviousActioningUser.LastName,
                            Email = task.PreviousActioningUser.Email
                        }
                        : null,
                    IsOpen = task.IsOpen,
                    IsCompleted = task.IsCompleted
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-task-data")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW, PrivilegeConstant.SEC_EMPLOYEE)]
    public async Task<IActionResult> GetTaskData()
    {
        try
        {
            var taskId = Request.Form["taskId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(taskId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            taskId = _cypherService.Decrypt(taskId);
            var task = await _taskService.FindByIdAsync(taskId);
            if (task == null)
                return Ok(new { success = false, message = "Task not found" });

            return Ok(new
            {
                success = true,
                task = new WkfTaskDto
                {
                    Id = _cypherService.Encrypt(task.Id),
                    Reference = task.Reference,
                    ParentTask = task.ParentTask != null
                        ? new WkfTaskDto
                        {
                            Id = _cypherService.Encrypt(task.ParentTask.Id),
                            Reference = task.ParentTask.Reference
                        }
                        : null,
                    CurrentProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(task.CurrentProcessId),
                        Name = task.CurrentProcess.Name,
                        Description = task.CurrentProcess.Description
                    },
                    CurrentProcessStartDate = task.CurrentProcessStartDate,
                    CurrentProcessEndDate = task.CurrentProcessEndDate,
                    CurrentStep = task.CurrentStep != null
                        ? new WkfProcessStepDto
                        {
                            Id = _cypherService.Encrypt(task.CurrentStep.Id),
                            Name = task.CurrentStep.Name,
                            Description = task.CurrentStep.Description
                        }
                        : null,
                    CurrentStepStartedDate = task.CurrentStepStartedDate,
                    CurrentStepExpectedEndDate = task.CurrentStepExpectedEndDate,
                    TaskStartedByUser = new UserDto
                    {
                        Id = _cypherService.Encrypt(task.TaskStartedByUserId),
                        FirstName = task.TaskStartedByUser.FirstName,
                        LastName = task.TaskStartedByUser.LastName,
                        FullName = task.TaskStartedByUser.FullName,
                        Email = task.TaskStartedByUser.Email
                    },
                    CurrentActioningUser = task.CurrentActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(task.CurrentActioningUserId),
                            FirstName = task.CurrentActioningUser.FirstName,
                            LastName = task.CurrentActioningUser.LastName,
                            FullName = task.CurrentActioningUser.FullName,
                            Email = task.CurrentActioningUser.Email
                        }
                        : null,
                    PreviousActioningUser = task.PreviousActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(task.PreviousActioningUserId),
                            FirstName = task.PreviousActioningUser.FirstName,
                            LastName = task.PreviousActioningUser.LastName,
                            Email = task.PreviousActioningUser.Email
                        }
                        : null,
                    IsOpen = task.IsOpen,
                    IsCompleted = task.IsCompleted,
                    WasSentBack = task.WasSentBack,
                    SentBackAtStep = task.SentBackAtStep != null
                        ? new WkfProcessStepDto
                        {
                            Id = _cypherService.Encrypt(task.SentBackAtStep.Id),
                            Name = task.SentBackAtStep.Name,
                            Description = task.SentBackAtStep.Description
                        }
                        : null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("close")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> CloseTask([FromForm] string reference)
    {
        try
        {
            if (string.IsNullOrEmpty(reference))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var task = await _taskService.FindByReferenceAsync(reference);
            if (task == null)
                return Ok(new { success = false, message = "Task not found" });

            if (!task.IsOpen)
                return Ok(new { success = false, message = "Task is already closed" });
            
            var currentDateTime = DateTimeOffset.Now;
            var currentUserId = _userManager.GetUserId(User);

            //save task log
            if (!string.IsNullOrEmpty(task.CurrentStepId))
            {
                await _taskLogService.CreateAsync(new WkfTaskLog
                {
                    TaskId = task.Id,
                    StepId = task.CurrentStepId,
                    ActioningUserId = currentUserId,
                    Action = WkfAction.Close.ToString(),
                    CreatedByUserId = currentUserId!,
                    CreatedDate = currentDateTime
                });
            }

            //close task
            task.IsOpen = false;
            task.CurrentProcessEndDate = currentDateTime;
            task.ModifiedByUserId = currentUserId;
            task.ModifiedDate = currentDateTime;
            await _taskService.UpdateAsync(task);
            return Ok(new
            {
                success = true, message = "Task with reference " + task.Reference + " has been closed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("reopen")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> ReopenTask([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var task = await _taskService.FindByIdAsync(id);
            if (task == null)
                return Ok(new { success = false, message = "Task not found" });

            if (task.IsOpen)
                return Ok(new { success = false, message = "Task is already open" });
            
            if (task.IsCompleted)
                return Ok(new { success = false, message = "A completed task cannot be reopened" });

            if (task.CurrentStep == null)
                return Ok(new { success = false, message = "Task has no step" });

            var currentDateTime = DateTimeOffset.Now;
            var currentUserId = _userManager.GetUserId(User);

            //save task log
            if (!string.IsNullOrEmpty(task.CurrentStepId))
            {
                await _taskLogService.CreateAsync(new WkfTaskLog
                {
                    TaskId = task.Id,
                    StepId = task.CurrentStepId,
                    ActioningUserId = currentUserId,
                    Action = WkfAction.Reopen.ToString(),
                    CreatedByUserId = currentUserId!,
                    CreatedDate = currentDateTime
                });
            }

            //reopen task
            task.IsOpen = true;
            task.ModifiedByUserId = currentUserId;
            task.ModifiedDate = currentDateTime;
            await _taskService.UpdateAsync(task);

            return Ok(task.CurrentActioningUser != null
                ? new
                {
                    success = true,
                    message = "Task reopened successfully.\nReference: " + task.Reference +
                              "\nCurrently assigned to: " + task.CurrentActioningUser.FullName
                }
                : new { success = true, message = "Task reopened successfully.\nReference: " + task.Reference });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("reassign")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> ReassignTask([FromForm] string id, [FromForm] string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            userId = _cypherService.Decrypt(userId);
            var task = await _taskService.FindByIdAsync(id);
            if (task == null)
                return Ok(new { success = false, message = "Task not found" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            if (task.CurrentActioningUserId == user.Id)
                return Ok(new { success = false, message = "Task is already assigned to " + user.FullName });

            if (!task.IsOpen)
                return Ok(new { success = false, message = "Task is closed" });
            
            if (task.IsCompleted)
                return Ok(new { success = false, message = "A completed task cannot be reassigned" });

            if (task.CurrentStep == null)
                return Ok(new { success = false, message = "Task has no step" });

            var currentDateTime = DateTimeOffset.Now;
            var currentUserId = _userManager.GetUserId(User);

            //save task log
            if (task.CurrentStep != null)
            {
                await _taskLogService.CreateAsync(new WkfTaskLog
                {
                    TaskId = task.Id,
                    Step = task.CurrentStep,
                    ActioningUserId = currentUserId,
                    NextActioningUser = user,
                    ActionDate = currentDateTime,
                    Action = WkfAction.Reassign.ToString(),
                    CreatedByUserId = currentUserId!,
                    CreatedDate = currentDateTime
                });
            }

            //reassign task
            task.CurrentActioningUserId = user.Id;
            task.ModifiedByUserId = currentUserId;
            task.ModifiedDate = currentDateTime;
            await _taskService.UpdateAsync(task);
            return Ok(new
            {
                success = true,
                message = "Task successfully reassigned to " + user.FullName + "\nReference: " + task.Reference
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-task-logs-dt
    [HttpPost("get-task-logs-dt")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetTaskLogsDt()
    {
        try
        {
            var taskId = Request.Form["taskId"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(taskId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            taskId = _cypherService.Decrypt(taskId);
            var taskLogs = await _taskLogService.FindByTaskIdAsync(taskId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                t => t.Task!.Reference!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.Step!.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.ActioningUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.Action!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                t => t,
                taskLogs.ToList().Select((taskLog, index) => new WkfTaskLogDto
                {
                    Id = _cypherService.Encrypt(taskLog.Id),
                    Counter = index + 1,
                    Task = new WkfTaskDto
                    {
                        Id = _cypherService.Encrypt(taskLog.TaskId),
                        Reference = taskLog.Task.Reference
                    },
                    Step = new WkfProcessStepDto
                    {
                        Id = _cypherService.Encrypt(taskLog.StepId),
                        Name = taskLog.Step.Name,
                        Description = taskLog.Step.Description
                    },
                    ActioningUser = taskLog.ActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(taskLog.ActioningUserId),
                            FirstName = taskLog.ActioningUser.FirstName,
                            LastName = taskLog.ActioningUser.LastName,
                            Email = taskLog.ActioningUser.Email
                        }
                        : null,
                    NextActioningUser = taskLog.NextActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(taskLog.NextActioningUserId),
                            FirstName = taskLog.NextActioningUser.FirstName,
                            LastName = taskLog.NextActioningUser.LastName,
                            Email = taskLog.NextActioningUser.Email
                        }
                        : null,
                    Action = taskLog.Action,
                    ActionDate = taskLog.ActionDate
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-child-tasks-dt
    [HttpPost("get-child-tasks-dt")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW)]
    public async Task<IActionResult> GetChildTasksDt()
    {
        try
        {
            var taskId = Request.Form["taskId"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(taskId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            taskId = _cypherService.Decrypt(taskId);
            var childTasks = await _taskService.GetChildTasksAsync(taskId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                t => t.Reference!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.CurrentProcess!.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.CurrentStep!.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.TaskStartedByUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.CurrentActioningUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     t.PreviousActioningUser!.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                t => t,
                childTasks.ToList().Select((task, index) => new WkfTaskDto
                {
                    Id = _cypherService.Encrypt(task.Id),
                    Counter = index + 1,
                    Reference = task.Reference,
                    ParentTask = task.ParentTask != null
                        ? new WkfTaskDto
                        {
                            Id = _cypherService.Encrypt(task.ParentTask.Id),
                            Reference = task.ParentTask.Reference
                        }
                        : null,
                    CurrentProcess = new WkfProcessDto
                    {
                        Id = _cypherService.Encrypt(task.CurrentProcessId),
                        Name = task.CurrentProcess.Name,
                        Description = task.CurrentProcess.Description
                    },
                    CurrentProcessStartDate = task.CurrentProcessStartDate,
                    CurrentStep = task.CurrentStep != null
                        ? new WkfProcessStepDto
                        {
                            Id = _cypherService.Encrypt(task.CurrentStep.Id),
                            Name = task.CurrentStep.Name,
                            Description = task.CurrentStep.Description
                        }
                        : null,
                    TaskStartedByUser = new UserDto
                    {
                        Id = _cypherService.Encrypt(task.TaskStartedByUserId),
                        FirstName = task.TaskStartedByUser.FirstName,
                        LastName = task.TaskStartedByUser.LastName,
                        Email = task.TaskStartedByUser.Email
                    },
                    CurrentActioningUser = task.CurrentActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(task.CurrentActioningUserId),
                            FirstName = task.CurrentActioningUser.FirstName,
                            LastName = task.CurrentActioningUser.LastName,
                            Email = task.CurrentActioningUser.Email
                        }
                        : null,
                    PreviousActioningUser = task.PreviousActioningUser != null
                        ? new UserDto
                        {
                            Id = _cypherService.Encrypt(task.PreviousActioningUserId),
                            FirstName = task.PreviousActioningUser.FirstName,
                            LastName = task.PreviousActioningUser.LastName,
                            Email = task.PreviousActioningUser.Email
                        }
                        : null,
                    IsOpen = task.IsOpen,
                    IsCompleted = task.IsCompleted
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}