using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Common.ApiControllers;

[ApiController]
[Area("Common")]
[Route("api/common/attachment")]
public class SystemAttachmentApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SystemAttachmentApiController> _logger;
    private readonly ISystemAttachmentManager _systemAttachmentManager;
    private readonly ISystemAttachmentService _systemAttachmentService;
    private readonly ICypherService _cypherService;
    private readonly IUserDetailService _userDetailService;

    public SystemAttachmentApiController(
        UserManager<ApplicationUser> userManager,
        ILogger<SystemAttachmentApiController> logger,
        ISystemAttachmentManager systemAttachmentManager,
        ISystemAttachmentService systemAttachmentService, ICypherService cypherService,
        IUserDetailService userDetailService)
    {
        _userManager = userManager;
        _logger = logger;
        _systemAttachmentManager = systemAttachmentManager;
        _systemAttachmentService = systemAttachmentService;
        _cypherService = cypherService;
        _userDetailService = userDetailService;
    }

    [HttpPost("upload-form-file")]
    public async Task<IActionResult> UploadFormFile()
    {
        try
        {
            var file = Request.Form.Files["file"];

            if (file == null)
                return Ok(new { success = false, message = "File is required" });

            var purpose = Request.Form["purpose"].FirstOrDefault();

            if (string.IsNullOrEmpty(purpose))
                return Ok(new { success = false, message = "Purpose is required" });

            var isValid = _systemAttachmentManager.IsValid(file);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dto = await _systemAttachmentManager.UploadFile(file, AttachmentDirectory.Common.GetDirectoryName());

            if (dto == null)
                return Ok(new { success = false, message = "Unable to upload file. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;
            var systemAttachment = new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _systemAttachmentService.CreateAsync(systemAttachment);

            var attachmentId = _cypherService.Encrypt(systemAttachment.Id);

            return Ok(new { success = true, message = "Attachment uploaded successfully", attachmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("upload-form-files")]
    public async Task<IActionResult> UploadFormFiles()
    {
        try
        {
            var files = Request.Form.Files;
            var purpose1 = Request.Form["purpose1"].FirstOrDefault();

            if (string.IsNullOrEmpty(purpose1))
                return Ok(new { success = false, message = "Purpose is required" });

            var isValid = _systemAttachmentManager.IsValid(files);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var dtos = _systemAttachmentManager.UploadFiles(files, "common");

            if (!dtos.Any())
                return Ok(new { success = false, message = "Unable to upload files. Please try again" });

            var currentUserId = _userManager.GetUserId(User);
            var currentDateTime = DateTimeOffset.UtcNow;

            var systemAttachments = dtos.Select(dto => new SystemAttachment
            {
                SystemFileName = dto.SystemFileName,
                CustomFileName = dto.CustomFileName,
                OriginalFileName = dto.OriginalFileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                UploadedByUserId = currentUserId!,
                DateUploaded = currentDateTime,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            }).ToList();

            await _systemAttachmentService.CreateAsync(systemAttachments);

            return Ok(new { success = true, message = "Attachments uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get attachment by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttachmentById(string id)
    {
        try
        {
            var attachmentId = _cypherService.Decrypt(id);

            var systemAttachment = await _systemAttachmentService.FindByIdAsync(attachmentId);

            if (systemAttachment == null)
                return Ok(new { success = false, message = "Attachment not found" });

            //return AttachmentDto
            var attachmentDto = new SystemAttachmentDto
            {
                FilePath = systemAttachment.FilePath,
                SystemFileName = systemAttachment.SystemFileName,
                CustomFileName = systemAttachment.CustomFileName,
                OriginalFileName = systemAttachment.OriginalFileName,
                FileSize = systemAttachment.FileSize,
                ContentType = systemAttachment.ContentType,
                FileExtension = systemAttachment.FileExtension
            };
            
            return Ok(new { success = true, attachment = attachmentDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }


    [HttpPost("get-user-profile-picture")]
    public async Task<IActionResult> GetUserProfilePicture()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var userDetail = await _userDetailService.FindByUserIdAsync(user.Id);
            if (userDetail == null)
                return Ok(new { success = false, message = "User details not found" });

            if (string.IsNullOrEmpty(userDetail.ProfilePictureAttachmentId))
                return Ok(new { success = true, filePath = "" });

            var profilePictureAttachment =
                await _systemAttachmentService.FindByIdAsync(userDetail.ProfilePictureAttachmentId);
            return profilePictureAttachment == null
                ? Ok(new { success = true, filePath = "" })
                : Ok(new { success = true, filePath = profilePictureAttachment.FilePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpGet("get-current-user-profile-picture")]
    public async Task<IActionResult> GetCurrentUserProfilePicture()
    {
        try
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var userDetail = await _userDetailService.FindByUserIdAsync(user.Id);
            if (userDetail == null)
                return Ok(new { success = false, message = "User details not found" });

            if (string.IsNullOrEmpty(userDetail.ProfilePictureAttachmentId))
                return Ok(new { success = true, filePath = "" });

            var profilePictureAttachment =
                await _systemAttachmentService.FindByIdAsync(userDetail.ProfilePictureAttachmentId);
            return profilePictureAttachment == null
                ? Ok(new { success = true, filePath = "" })
                : Ok(new { success = true, filePath = profilePictureAttachment.FilePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}