using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Workflow.Dto;
using zamren_management_system.Areas.Workflow.Interfaces;

namespace zamren_management_system.Areas.Workflow.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN, EMPLOYEE")]
[Area("Workflow")]
[Route("api/workflow/task/comment")]
public class TaskCommentApiController : ControllerBase
{
    private readonly ILogger<TaskCommentApiController> _logger;
    private readonly ICypherService _cypherService;
    private readonly ITaskService _taskService;
    private readonly IUtil _util;

    public TaskCommentApiController(ILogger<TaskCommentApiController> logger, ICypherService cypherService,
        ITaskService taskService, IUtil util)
    {
        _logger = logger;
        _cypherService = cypherService;
        _taskService = taskService;
        _util = util;
    }

    [HttpPost("get-by-reference")]
    [HasPrivilege(PrivilegeConstant.WKF_MANAGE_SYSTEM_WORKFLOW, PrivilegeConstant.SEC_EMPLOYEE)]
    public async Task<IActionResult> GetTaskCommentsByReference()
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

            if (task.WkfTaskComments == null)
                return Ok(new { success = true, task, comments = new List<WkfCommentDto>() });

            //sorting comments by commented date in descending order
            task.WkfTaskComments = task.WkfTaskComments.OrderByDescending(c => c.CommentedDate).ToList();

            var comments = task.WkfTaskComments?.Select(comment => new WkfCommentDto
            {
                Id = _cypherService.Encrypt(comment.Id),
                Comment = comment.Comment,
                Step = new WkfProcessStepDto
                {
                    Id = _cypherService.Encrypt(comment.Step.Id),
                    Name = comment.Step.Name
                },
                CommentedBy = new UserDto
                {
                    Id = _cypherService.Encrypt(comment.CommentedBy.Id),
                    FullName = comment.CommentedBy.FullName,
                    FirstName = comment.CommentedBy.FirstName,
                    LastName = comment.CommentedBy.LastName,
                    Email = comment.CommentedBy.Email
                },
                CommentedDate = comment.CommentedDate,
                TimeAgo = _util.GetTimeAgoInWords(comment.CommentedDate),
                WkfAttachments = comment.WkfTaskAttachments?.Select(attachment => new WkfAttachmentDto
                {
                    Id = _cypherService.Encrypt(attachment.Id),
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
                }).ToList()
            }).ToList();

            //mark the most recent comment with a flag: MostRecent = true
            /*if (comments is { Count: > 1 })
            {
                var dto = comments.FirstOrDefault();
                if (dto != null) dto.MostRecent = true;
            }*/

            return Ok(new { success = true, task, comments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}