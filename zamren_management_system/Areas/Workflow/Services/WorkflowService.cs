using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Common.Responses;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Workflow.Enums;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AuthContext _context;
    private readonly ILogger<WorkflowService> _logger;
    private readonly IStepService _stepService;
    private readonly IProcessService _processService;
    private readonly ITaskService _taskService;
    private readonly ITaskLogService _taskLogService;
    private readonly ITaskCommentService _taskCommentService;
    private readonly ITaskAttachmentService _taskAttachmentService;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailTemplate _emailTemplate;
    private readonly ICypherService _cypherService;
    private readonly ISystemNotificationService _systemNotificationService;

    public WorkflowService(AuthContext context, ILogger<WorkflowService> logger, IStepService stepService,
        IProcessService processService, ITaskService taskService, ITaskLogService taskLogService,
        ITaskCommentService taskCommentService, ITaskAttachmentService taskAttachmentService, IEmailSender emailSender,
        IEmailTemplate emailTemplate, ICypherService cypherService, UserManager<ApplicationUser> userManager,
        ISystemNotificationService systemNotificationService)
    {
        _context = context;
        _logger = logger;
        _stepService = stepService;
        _processService = processService;
        _taskService = taskService;
        _taskLogService = taskLogService;
        _taskCommentService = taskCommentService;
        _taskAttachmentService = taskAttachmentService;
        _emailSender = emailSender;
        _emailTemplate = emailTemplate;
        _cypherService = cypherService;
        _userManager = userManager;
        _systemNotificationService = systemNotificationService;
    }

    public async Task<string> GenerateReferenceNumber(string moduleCode)
    {
        try
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "generate_reference_number";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "i_module_code";
            parameter.Value = moduleCode;
            command.Parameters.Add(parameter);

            var outRefNoParam = command.CreateParameter();
            outRefNoParam.ParameterName = "o_ref_no";
            outRefNoParam.DbType = DbType.String;
            outRefNoParam.Size = 50;
            outRefNoParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(outRefNoParam);

            await _context.Database.OpenConnectionAsync();

            await command.ExecuteNonQueryAsync();

            if (outRefNoParam.Value == null)
            {
                _logger.LogError("Reference number could not be generated.");
                throw new Exception("Reference number could not be generated.");
            }

            var referenceNumber = (string)outRefNoParam.Value;

            _logger.LogInformation("Reference number generated successfully. Reference Number: {referenceNumber}",
                referenceNumber);

            return referenceNumber;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while generating reference number.");
            throw new Exception(e.Message);
        }
    }

    public async Task<(WkfTask? task, CustomIdentityResult response)> InitiateWorkflowTask(
        string reference,
        string currentUserId,
        string processId,
        string? comment = null,
        List<SystemAttachment>? systemAttachments = null,
        string? parentReference = null
    )
    {
        if (string.IsNullOrEmpty(currentUserId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Current user not found" }));

        if (string.IsNullOrEmpty(processId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Process not found" }));

        //get the process then get module code
        var process = await _processService.FindByIdAsync(processId);
        if (process == null)
            return (null, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "Process not found"
            }));

        if (string.IsNullOrEmpty(reference))
            reference = await GenerateReferenceNumber(process.Module.Code);

        //get first step
        var step = await _stepService.GetFirstStepAsync(processId);

        if (step == null)
            return (null, CustomIdentityResult.Failed(new IdentityError
            {
                Description = "First step not found"
            }));

        //get actioning user
        var actioningUser = await _stepService.FindActioningUserAsync(step);
        var currentDateTime = DateTimeOffset.Now;

        //save task
        var task = new WkfTask();

        //get parent task
        if (!string.IsNullOrEmpty(parentReference))
        {
            var parentTask = await _taskService.FindByReferenceAsync(parentReference);
            if (parentTask == null)
                return (null, CustomIdentityResult.Failed(new IdentityError
                {
                    Description = "Parent task not found"
                }));

            task.ParentTask = parentTask;
        }

        if (step.SlaHours != null)
            task.CurrentStepExpectedEndDate = currentDateTime.AddHours((double)step.SlaHours);

        task.PreviousActioningUser = null;
        task.CurrentActioningUser = actioningUser;
        task.Reference = reference;
        task.CurrentProcessId = processId;
        task.CurrentProcessStartDate = currentDateTime;
        task.CurrentStepStartedDate = currentDateTime;
        task.CurrentStepId = step.Id;
        task.TaskStartedByUserId = currentUserId;
        task.CreatedByUserId = currentUserId;
        task.CreatedDate = currentDateTime;
        task.IsOpen = true;
        task.WasSentBack = false;

        var response1 = await _taskService.CreateAsync(task);
        if (!response1.Succeeded)
            return (null, CustomIdentityResult.Failed(new IdentityError
            {
                Description = response1.Errors.First().Description
            }));

        //save task log
        await SaveTaskLog(currentUserId, task, currentDateTime, WkfAction.Initiate, task.CurrentActioningUser);

        //save task comment
        if (!string.IsNullOrEmpty(comment))
        {
            var response = await SaveComment(currentUserId, comment, task, currentDateTime);
            if (!response.Succeeded)
                return (null, CustomIdentityResult.Failed(new IdentityError
                {
                    Description = "An error occurred while saving task comment"
                }));
        }

        //save task attachment 
        if (systemAttachments != null && systemAttachments.Any())
        {
            var response = await SaveAttachments(currentUserId, systemAttachments, task, step, currentDateTime);
            if (!response.Succeeded)
                return (null, CustomIdentityResult.Failed(new IdentityError
                {
                    Description = "An error occurred while saving task attachments"
                }));
        }

        if (task.CurrentActioningUser == null)
            return (task, CustomIdentityResult.Success("Task initiated successfully.\nReference Number: " +
                                                       task.Reference));

        var requestMap = step.RequestMap;
        var url = "/Workflow/Tasks/" + _cypherService.Encrypt(task.Reference);

        if (!string.IsNullOrEmpty(requestMap))
        {
            if (requestMap[^1] != '/')
                requestMap += "/";

            url = requestMap + _cypherService.Encrypt(task.Reference);
        }

        //email user
        var body = _emailTemplate.AssignedWorkflowTask(
            task.CurrentActioningUser.FirstName,
            task.Reference,
            step.Name,
            url
        );

        await _emailSender.SendEmailAsync(
            task.CurrentActioningUser.Email!,
            "New Task",
            body,
            currentUserId
        );


        //save system notification
        var response4 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = task.CurrentActioningUser.Id,
            Subject = "New Task",
            Body = "A task has been assigned to you with reference number " + task.Reference,
            ActionUrl = url,
            ActionUrlText = "View Task",
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response4.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));


        return (task, CustomIdentityResult
            .Success("Task initiated successfully and assigned to " + task.CurrentActioningUser.FullName +
                     "\nReference Number: " + task.Reference));
    }

    public async Task<(WkfTask? task, CustomIdentityResult response)> ApproveWorkflowTask(
        string reference,
        string currentUserId,
        string? comment = null,
        List<SystemAttachment>? systemAttachments = null
    )
    {
        if (string.IsNullOrEmpty(reference))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Reference not found" }));

        if (string.IsNullOrEmpty(currentUserId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Current user not found" }));

        var task = await _taskService.FindByReferenceAsync(reference);
        if (task == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task not found" }));

        if (!task.IsOpen)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task is closed" }));

        if (task.CurrentStep == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Current step not found" }));

        var currentDateTime = DateTimeOffset.Now;
        const WkfAction action = WkfAction.Approve;

        // Handle comments
        if (!string.IsNullOrEmpty(comment))
        {
            var response = await SaveComment(currentUserId, comment, task, currentDateTime);
            if (!response.Succeeded)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save comment" }));
        }

        // Handle attachments
        if (systemAttachments != null && systemAttachments.Any())
        {
            var response =
                await SaveAttachments(currentUserId, systemAttachments, task, task.CurrentStep, currentDateTime);
            if (!response.Succeeded)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save attachments" }));
        }

        //if current step is the step when task was sent back, then mark it as not sent back
        if (task.WasSentBack && task.CurrentStep == task.SentBackAtStep)
        {
            task.WasSentBack = false;
            task.SentBackAtStep = null;
        }

        //approve task
        if (task.CurrentStep.NextStep == null || task.CurrentStep.IsFinalStep) //if task is at final step, close task
        {
            //save task log before closing task
            await SaveTaskLog(currentUserId, task, currentDateTime, action, null);

            var previousActioningUser = task.CurrentActioningUser;

            //close task
            task.IsOpen = false;
            task.CurrentStep = null;
            task.PreviousActioningUser = previousActioningUser;
            task.CurrentActioningUser = null;
            task.CurrentProcessEndDate = currentDateTime;
            task.ModifiedByUserId = currentUserId;
            task.ModifiedDate = currentDateTime;

            var response3 = await _taskService.UpdateAsync(task);
            if (!response3.Succeeded)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError { Description = "Failed to update task" }));
        }
        else //if task is not at final step, move task to next step
        {
            var nextStep = task.CurrentStep.NextStep;

            var actioningUser =
                await _stepService.FindActioningUserAsync(nextStep);

            //save task log before closing task
            await SaveTaskLog(currentUserId, task, currentDateTime, action, actioningUser);

            var previousActioningUser = task.CurrentActioningUser;

            task.CurrentStep = nextStep;
            task.PreviousActioningUser = previousActioningUser;
            task.CurrentActioningUser = actioningUser;
            task.CurrentStepStartedDate = currentDateTime;
            task.ModifiedByUserId = currentUserId;
            task.ModifiedDate = currentDateTime;

            var response3 = await _taskService.UpdateAsync(task);
            if (!response3.Succeeded)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError { Description = "Failed to update task" }));
        }

        if (task.CurrentActioningUser == null || task.CurrentStep == null)
            return (task, CustomIdentityResult.Success("Task approved successfully.\nReference Number: " +
                                                       task.Reference));

        var requestMap = task.CurrentStep.RequestMap;
        var url = "/Workflow/Tasks/" + _cypherService.Encrypt(task.Reference);

        if (!string.IsNullOrEmpty(requestMap))
        {
            if (requestMap[^1] != '/')
                requestMap += "/";

            url = requestMap + _cypherService.Encrypt(task.Reference);
        }

        //email user
        var body = _emailTemplate.AssignedWorkflowTask(
            task.CurrentActioningUser.FirstName,
            task.Reference,
            task.CurrentStep.Name,
            url
        );

        await _emailSender.SendEmailAsync(
            task.CurrentActioningUser.Email!,
            "New Task",
            body,
            currentUserId
        );

        //save system notification
        var response4 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = task.CurrentActioningUser.Id,
            Subject = "New Task",
            Body = "A task has been assigned to you with reference number " + task.Reference,
            ActionUrl = url,
            ActionUrlText = "View Task",
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response4.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));

        return (task, CustomIdentityResult.Success("Task approved successfully and assigned to " +
                                                   task.CurrentActioningUser.FullName + "\nReference Number: " +
                                                   task.Reference));
    }

    //move the task to next or previous step
    public async Task<(WkfTask? task, CustomIdentityResult response)> SendBackWorkflowTask(
        string reference,
        string currentUserId,
        string? comment = null,
        List<SystemAttachment>? systemAttachments = null
    )
    {
        if (string.IsNullOrEmpty(reference))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Reference not found" }));

        if (string.IsNullOrEmpty(currentUserId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Current user not found" }));

        var task = await _taskService.FindByReferenceAsync(reference);
        if (task == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task not found" }));

        if (!task.IsOpen)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task is closed" }));

        if (task.CurrentStep == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Current step not found" }));

        if (task.CurrentStep.IsInitialStep || string.IsNullOrEmpty(task.CurrentStep.PreviousStepId) ||
            task.CurrentStep.PreviousStep == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Previous step not found" }));

        var currentDateTime = DateTimeOffset.Now;
        const WkfAction action = WkfAction.SendBack;

        // Handle comments
        if (!string.IsNullOrEmpty(comment))
        {
            var response = await SaveComment(currentUserId, comment, task, currentDateTime);
            if (!response.Succeeded)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save comment" }));
        }

        // Handle attachments
        if (systemAttachments != null && systemAttachments.Any())
        {
            var response =
                await SaveAttachments(currentUserId, systemAttachments, task, task.CurrentStep, currentDateTime);
            if (!response.Succeeded)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save attachments" }));
        }

        //send task back
        task.SentBackAtStep = task.CurrentStep;
        var previousStep = task.CurrentStep.PreviousStep;

        var actioningUser =
            await _stepService.FindActioningUserAsync(previousStep);

        await SaveTaskLog(currentUserId, task, currentDateTime, action, actioningUser);

        var previousActioningUser = task.CurrentActioningUser;

        task.CurrentStep = previousStep;
        task.PreviousActioningUser = previousActioningUser;
        task.CurrentActioningUser = actioningUser;
        task.WasSentBack = true;
        task.CurrentStepStartedDate = currentDateTime;
        task.ModifiedByUserId = currentUserId;
        task.ModifiedDate = currentDateTime;

        var response3 = await _taskService.UpdateAsync(task);
        if (!response3.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to update task" }));

        if (task.CurrentActioningUser == null)
            return (task, CustomIdentityResult.Success("Task successfully sent back" +
                                                       "\nReference Number: " + task.Reference));

        var requestMap = task.CurrentStep.RequestMap;
        var url = "/Workflow/Tasks/" + _cypherService.Encrypt(task.Reference);

        if (!string.IsNullOrEmpty(requestMap))
        {
            if (requestMap[^1] != '/')
                requestMap += "/";

            url = requestMap + _cypherService.Encrypt(task.Reference);
        }

        //email user
        var body = _emailTemplate.AssignedWorkflowTask(
            task.CurrentActioningUser.FirstName,
            task.Reference,
            task.CurrentStep.Name,
            url
        );

        await _emailSender.SendEmailAsync(
            task.CurrentActioningUser.Email!,
            "New Task",
            body,
            currentUserId
        );

        //save system notification
        var response4 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = task.CurrentActioningUser.Id,
            Subject = "New Task",
            Body = "A task has been assigned to you with reference number " + task.Reference,
            ActionUrl = url,
            ActionUrlText = "View Task",
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response4.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));

        return (task, CustomIdentityResult.Success("Task successfully sent back to " +
                                                   task.CurrentActioningUser.FullName + "\nReference Number: " +
                                                   task.Reference));
    }


    //reassign task
    public async Task<(WkfTask? task, CustomIdentityResult response)> ReassignWorkflowTask(
        string reference,
        string nextActioningUserId,
        string currentUserId
    )
    {
        if (string.IsNullOrEmpty(reference))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Reference not found" }));

        if (string.IsNullOrEmpty(nextActioningUserId))
            return (null,
                CustomIdentityResult.Failed(new IdentityError { Description = "Next actioning user not found" }));

        if (string.IsNullOrEmpty(currentUserId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Current user not found" }));

        var task = await _taskService.FindByReferenceAsync(reference);
        if (task == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task not found" }));

        if (!task.IsOpen)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task is closed" }));

        if (task.CurrentStep == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task is closed" }));

        if (task.CurrentActioningUserId == nextActioningUserId)
            return (task,
                CustomIdentityResult.Failed(new IdentityError
                    { Description = "Task is already assigned to this user" }));

        var nextActioningUser = await _userManager.FindByIdAsync(nextActioningUserId);
        if (nextActioningUser == null)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "User not found" }));

        var currentDateTime = DateTimeOffset.Now;
        const WkfAction action = WkfAction.Reassign;

        //save task log
        await SaveTaskLog(currentUserId, task, currentDateTime, action, nextActioningUser);

        //old user
        var oldUser = task.CurrentActioningUser;

        //reassign task
        task.CurrentActioningUser = nextActioningUser;
        task.ModifiedByUserId = currentUserId;
        task.ModifiedDate = currentDateTime;

        var response3 = await _taskService.UpdateAsync(task);
        if (!response3.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to update task" }));

        var requestMap = task.CurrentStep.RequestMap;
        var url = "/Workflow/Tasks/" + _cypherService.Encrypt(task.Reference);

        if (!string.IsNullOrEmpty(requestMap))
        {
            if (requestMap[^1] != '/')
                requestMap += "/";

            url = requestMap + _cypherService.Encrypt(task.Reference);
        }

        //email user
        var body = _emailTemplate.AssignedWorkflowTask(
            task.CurrentActioningUser.FirstName,
            task.Reference,
            task.CurrentStep.Name,
            url
        );

        await _emailSender.SendEmailAsync(
            task.CurrentActioningUser.Email!,
            "New Task",
            body,
            currentUserId
        );

        //save system notification
        var response4 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = task.CurrentActioningUser.Id,
            Subject = "New Task",
            Body = "A task has been assigned to you with reference number " + task.Reference,
            ActionUrl = url,
            ActionUrlText = "View Task",
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response4.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));

        if (oldUser == null)
            return (task,
                CustomIdentityResult.Success("Task successfully reassigned to " + task.CurrentActioningUser?.FullName +
                                             "\nReference Number: " + task.Reference));

        //email old user
        var body2 = _emailTemplate.UnassignedWorkflowTask(
            oldUser.FirstName,
            task.CurrentActioningUser.FullName,
            task.Reference,
            task.CurrentStep.Name
        );

        await _emailSender.SendEmailAsync(
            oldUser.Email!,
            "Task Reassigned",
            body2,
            currentUserId
        );

        //save system notification
        var response5 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = oldUser.Id,
            Subject = "Task Reassigned",
            Body = "A task has been reassigned from you to " + task.CurrentActioningUser.FullName +
                   " with reference number " + task.Reference,
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response5.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));

        return (task,
            CustomIdentityResult.Success("Task successfully reassigned from " + oldUser.FullName + " to " +
                                         task.CurrentActioningUser.FullName + ".\nReference Number: " +
                                         task.Reference));
    }


    //close task
    public async Task<(WkfTask? task, CustomIdentityResult response)> CloseWorkflowTask(
        string reference,
        string currentUserId
    )
    {
        if (string.IsNullOrEmpty(reference))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Reference not found" }));

        if (string.IsNullOrEmpty(currentUserId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Current user not found" }));

        var task = await _taskService.FindByReferenceAsync(reference);
        if (task == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task not found" }));

        // if (task.CurrentStep == null)
            // return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task is closed" }));

        if (!task.IsOpen)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task is already closed" }));

        var currentDateTime = DateTimeOffset.Now;
        const WkfAction action = WkfAction.Close;

        //save task log
        await SaveTaskLog(currentUserId, task, currentDateTime, action, null);

        //close task
        task.IsOpen = false;
        task.CurrentProcessEndDate = currentDateTime;
        task.ModifiedByUserId = currentUserId;
        task.ModifiedDate = currentDateTime;

        var response3 = await _taskService.UpdateAsync(task);
        if (!response3.Succeeded)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Failed to update task" }));

        //send email to task starter
        var body = _emailTemplate.WorkflowTaskClosed(
            task.TaskStartedByUser.FirstName,
            task.Reference,
            task.CurrentProcess.Name
        );

        await _emailSender.SendEmailAsync(
            task.TaskStartedByUser.Email!,
            "Task Closed",
            body,
            currentUserId
        );

        //save system notification
        var response4 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = task.TaskStartedByUserId,
            Subject = "Task Closed",
            Body = "The task you initiated has been closed. Reference number " + task.Reference,
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response4.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));

        return (task, CustomIdentityResult.Success("Task closed successfully.\nReference Number: " + task.Reference));
    }

    //re-open task
    public async Task<(WkfTask? task, CustomIdentityResult response)> ReopenWorkflowTask(
        string reference,
        string currentUserId
    )
    {
        if (string.IsNullOrEmpty(reference))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Reference not found" }));

        if (string.IsNullOrEmpty(currentUserId))
            return (null, CustomIdentityResult.Failed(new IdentityError { Description = "Current user not found" }));

        var task = await _taskService.FindByReferenceAsync(reference);
        if (task == null)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Task not found" }));

        if (task.IsOpen)
        {
            if (task.CurrentActioningUser != null)
                return (task,
                    CustomIdentityResult.Failed(new IdentityError
                    {
                        Description = "Task is already open and assigned to " + task.CurrentActioningUser.FullName
                    }));

            return (task,
                CustomIdentityResult.Failed(new IdentityError
                    { Description = "Task is already open but not assigned to anyone" }));
        }

        if (task.CurrentStep == null)
            return (task,
                CustomIdentityResult.Failed(new IdentityError
                    { Description = "Task was completed and cannot be reopened" }));

        var currentDateTime = DateTimeOffset.Now;
        const WkfAction action = WkfAction.Reopen;

        //save task log
        await SaveTaskLog(currentUserId, task, currentDateTime, action, task.CurrentActioningUser);

        //reopen task
        task.IsOpen = true;
        task.CurrentProcessEndDate = null;
        task.ModifiedByUserId = currentUserId;
        task.ModifiedDate = currentDateTime;

        var response3 = await _taskService.UpdateAsync(task);
        if (!response3.Succeeded)
            return (task, CustomIdentityResult.Failed(new IdentityError { Description = "Failed to update task" }));

        //send email to task starter
        var body = _emailTemplate.WorkflowTaskReopened(
            task.TaskStartedByUser.FirstName,
            task.Reference,
            task.CurrentProcess.Name
        );

        await _emailSender.SendEmailAsync(
            task.TaskStartedByUser.Email!,
            "Task Reopened",
            body,
            currentUserId
        );

        //save system notification
        var response4 = await _systemNotificationService.CreateAsync(new SystemNotification
        {
            RecipientUserId = task.TaskStartedByUserId,
            Subject = "Task Reopened",
            Body = "The task you initiated has been reopened. Reference number " + task.Reference,
            DatePosted = currentDateTime,
            NotificationType = SystemNotificationType.Information.ToString(),
            Priority = (int)SystemNotificationPriority.Medium,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        });

        if (!response4.Succeeded)
            return (task,
                CustomIdentityResult.Failed(new IdentityError { Description = "Failed to save system notification" }));

        return (task, CustomIdentityResult.Success("Task reopened successfully.\nReference Number: " + task.Reference));
    }

    private async Task SaveTaskLog(string currentUserId, WkfTask task, DateTimeOffset currentDateTime,
        WkfAction action, ApplicationUser? nextActioningUser)
    {
        try
        {
            if (string.IsNullOrEmpty(task.Id))
                throw new Exception("Task not found");

            if (string.IsNullOrEmpty(currentUserId))
                throw new Exception("Current user not found");

            if (task.CurrentStep == null)
                throw new Exception("Current step not found");

            var taskLog = new WkfTaskLog
            {
                TaskId = task.Id,
                Step = task.CurrentStep,
                ActioningUserId = currentUserId,
                NextActioningUser = nextActioningUser,
                Action = action.ToString(),
                ActionDate = currentDateTime,
                CreatedByUserId = currentUserId,
                CreatedDate = currentDateTime
            };

            await _taskLogService.CreateAsync(taskLog);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while saving task log");
        }
    }

    private async Task<IdentityResult> SaveComment(string currentUserId, string comment, WkfTask task,
        DateTimeOffset currentDateTime)
    {
        var taskComment = new WkfTaskComment
        {
            TaskId = task.Id,
            StepId = task.CurrentStepId!,
            Comment = comment,
            CommentedByUserId = currentUserId,
            CreatedByUserId = currentUserId,
            CreatedDate = currentDateTime
        };

        var response3 = await _taskCommentService.CreateAsync(taskComment);
        return response3;
    }

    private async Task<IdentityResult> SaveAttachments(string currentUserId, List<SystemAttachment> systemAttachments,
        WkfTask task,
        WkfProcessStep nextStep, DateTimeOffset currentDateTime)
    {
        var taskAttachments = new List<WkfTaskAttachment>();
        foreach (var attachment in systemAttachments)
        {
            var taskAttachment = new WkfTaskAttachment
            {
                TaskId = task.Id,
                StepId = nextStep.Id,
                AttachmentId = attachment.Id,
                CreatedByUserId = currentUserId,
                CreatedDate = currentDateTime
            };
            taskAttachments.Add(taskAttachment);
        }

        var response = await _taskAttachmentService.CreateAsync(taskAttachments);
        return response;
    }
}