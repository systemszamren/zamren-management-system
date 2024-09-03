/*using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Workflow.Dto;
using zamren_management_system.Areas.Workflow.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace zamren_management_system.Areas.Workflow.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN, EMPLOYEE")]
[Area("Workflow")]
[Route("api/workflow/task/attachment")]
public class TaskAttachmentApiController : ControllerBase
{
    private readonly ILogger<TaskAttachmentApiController> _logger;
    private readonly ITaskService _taskService;
    private readonly ICypherService _cypherService;

    public TaskAttachmentApiController(ILogger<TaskAttachmentApiController> logger,
        ITaskService taskService, ICypherService cypherService)
    {
        _logger = logger;
        _taskService = taskService;
        _cypherService = cypherService;
    }

    [HttpPost("get-by-reference")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW, PrivilegeConstant.SEC_EMPLOYEE)]
    public async Task<IActionResult> GetTaskAttachmentsByReference()
    {
        try
        {
            var reference = Request.Form["reference"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(reference) || reference.Equals("null"))
                return Ok(new { success = false, message = "Invalid reference" });

            reference = _cypherService.Decrypt(reference);
            var task = await _taskService.FindByReferenceAsync(reference);
            if (task == null)
                return Ok(new { success = false, message = "Task not found" });

            var attachments = task.WkfTaskAttachments?.Select(attachment => new WkfAttachmentDto
            {
                Id = _cypherService.Encrypt(attachment.Id),
                Task = new WkfTaskDto
                {
                    Id = _cypherService.Encrypt(attachment.Task.Id),
                    Reference = attachment.Task.Reference
                },
                Step = new WkfProcessStepDto
                {
                    Id = _cypherService.Encrypt(attachment.Step.Id),
                    Name = attachment.Step.Name,
                    Description = attachment.Step.Description
                },
                SystemAttachment = new SystemAttachmentDto
                {
                    Id = _cypherService.Encrypt(attachment.Attachment.Id),
                    SystemFileName = attachment.Attachment.SystemFileName,
                    CustomFileName = attachment.Attachment.CustomFileName,
                    OriginalFileName = attachment.Attachment.OriginalFileName,
                    FilePath = attachment.Attachment.FilePath,
                    FileSize = attachment.Attachment.FileSize,
                    ContentType = attachment.Attachment.ContentType,
                    FileExtension = attachment.Attachment.FileExtension
                }
            }).ToList();

            return Ok(new { success = true, attachments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}*/