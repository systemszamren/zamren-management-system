using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Dto.PurchaseRequisitions;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;
using zamren_management_system.Areas.Procurement.Models;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Workflow.Dto;
using zamren_management_system.Areas.Workflow.Interfaces;

namespace zamren_management_system.Areas.Procurement.ApiControllers.PurchaseRequisitions;

[ApiController]
[Authorize(Roles = "EMPLOYEE")]
[Area("Procurement")]
[Route("api/procurement/purchase-requisition-good")]
public class PurchaseRequisitionApiController : ControllerBase
{
    private readonly ILogger<PurchaseRequisitionApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICypherService _cypherService;
    private readonly IWorkflowService _workflowService;
    private readonly ISystemAttachmentManager _systemAttachmentManager;
    private readonly ISystemAttachmentService _systemAttachmentService;
    private readonly IPurchaseRequisitionService _purchaseRequisitionService;
    private readonly IProcessService _processService;
    private readonly IOfficeService _officeService;
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly IOrganizationService _organizationService;
    private readonly IPurchaseRequisitionAttachmentService _purchaseRequisitionAttachmentService;

    public PurchaseRequisitionApiController(ILogger<PurchaseRequisitionApiController> logger,
        UserManager<ApplicationUser> userManager, ICypherService cypherService,
        ISystemAttachmentService systemAttachmentService, ISystemAttachmentManager systemAttachmentManager,
        IWorkflowService workflowService, IPurchaseRequisitionService purchaseRequisitionService,
        IOfficeService officeService, IBranchService branchService, IDepartmentService departmentService,
        IOrganizationService organizationService, IProcessService processService,
        IPurchaseRequisitionAttachmentService purchaseRequisitionAttachmentService)
    {
        _logger = logger;
        _userManager = userManager;
        _cypherService = cypherService;
        _systemAttachmentService = systemAttachmentService;
        _systemAttachmentManager = systemAttachmentManager;
        _workflowService = workflowService;
        _purchaseRequisitionService = purchaseRequisitionService;
        _officeService = officeService;
        _branchService = branchService;
        _departmentService = departmentService;
        _organizationService = organizationService;
        _processService = processService;
        _purchaseRequisitionAttachmentService = purchaseRequisitionAttachmentService;
    }

    [HttpPost("get-purchase-requisition-data")]
    [HasPrivilege(PrivilegeConstant.SEC_EMPLOYEE)]
    public async Task<IActionResult> GetInitiatePurchaseRequisitionData()
    {
        try
        {
            // Step 1: Get reference number from URL parameter
            var reference = Request.Form["reference"].FirstOrDefault() ?? "";

            PurchaseRequisitionDto? purchaseRequisitionDto = null;

            // Step 2: Get purchase requisition by reference number if reference number is not empty
            if (!string.IsNullOrEmpty(reference) && !reference.Equals("null"))
            {
                reference = _cypherService.Decrypt(reference);
                var purchaseRequisition = await _purchaseRequisitionService.FindByReferenceAsync(reference);
                if (purchaseRequisition == null)
                    return Ok(new { success = false, message = "Purchase requisition not found" });

                purchaseRequisitionDto = new PurchaseRequisitionDto
                {
                    Id = purchaseRequisition.Id,
                    Reference = purchaseRequisition.Reference,
                    OrganizationId = purchaseRequisition.OrganizationId,
                    BranchId = purchaseRequisition.BranchId,
                    DepartmentId = purchaseRequisition.DepartmentId,
                    OfficeId = purchaseRequisition.OfficeId,
                    // ItemDescription = purchaseRequisition.ItemDescription,
                    // Quantity = purchaseRequisition.Quantity,
                    // EstimatedCost = purchaseRequisition.EstimatedCost,
                    Justification = purchaseRequisition.Justification
                };

                //get purchase requisition attachments
                var purchaseRequisitionAttachments =
                    await _purchaseRequisitionAttachmentService.FindByPurchaseRequisitionIdAsync(
                        purchaseRequisition.Id);

                purchaseRequisitionDto.PurchaseRequisitionAttachments = purchaseRequisitionAttachments
                    .Select(attachment => new PurchaseRequisitionAttachmentDto
                    {
                        Id = _cypherService.Encrypt(attachment.Id),
                        PlainId = attachment.Id,
                        PurchaseRequisitionDto = purchaseRequisitionDto,
                        SystemAttachment = new SystemAttachmentDto
                        {
                            Id = _cypherService.Encrypt(attachment.SystemAttachment.Id),
                            FilePath = attachment.SystemAttachment.FilePath,
                            SystemFileName = attachment.SystemAttachment.SystemFileName,
                            CustomFileName = attachment.SystemAttachment.CustomFileName,
                            OriginalFileName = attachment.SystemAttachment.OriginalFileName,
                            FileSize = attachment.SystemAttachment.FileSize,
                            ContentType = attachment.SystemAttachment.ContentType,
                            FileExtension = attachment.SystemAttachment.FileExtension
                        }
                    }).ToList();
            }

            // Step 3: If reference number is empty, get current user details
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            var userDto = new UserDto
            {
                Id = _cypherService.Encrypt(user.Id),
                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            // Step 4: Get organizations
            var organizations = await _organizationService.FindAllAsync();

            // Step 5: Get branches in the organization
            var branches = await _branchService.FindAllAsync();

            // Step 6: Get departments in the branch
            var departments = await _departmentService.FindAllAsync();

            // Step 7: Get offices in the department
            var offices = await _officeService.FindAllAsync();

            //get current user's office, department, branch and organization from userOffice
            if (purchaseRequisitionDto == null)
            {
                var userOffice = await _officeService.FindOfficeByUserIdAsync(user.Id);
                if (userOffice != null)
                {
                    purchaseRequisitionDto = new PurchaseRequisitionDto
                    {
                        OrganizationId = userOffice.Office.Department.Branch.OrganizationId,
                        BranchId = userOffice.Office.Department.BranchId,
                        DepartmentId = userOffice.Office.DepartmentId,
                        OfficeId = userOffice.OfficeId
                    };
                }
            }

            //get process by ID
            var processId = ProcessConstant.PROC_PURCHASE_REQUISITION_OF_GOODS.GetProcessId()!;
            var currentProcess = await _processService.FindByIdAsync(processId);
            if (currentProcess == null)
                return Ok(new { success = false, message = "Process not found" });

            //get process 1st step Id
            var currentStep = await _processService.GetFirstStepAsync(processId);
            if (currentStep == null)
                return Ok(new { success = false, message = "First step not found" });

            return Ok(new
            {
                success = true,
                currentProcess = new WkfProcessDto
                {
                    Id = _cypherService.Encrypt(currentProcess.Id),
                    Name = currentProcess.Name,
                    Description = currentProcess.Description
                },
                currentStep = new WkfProcessStepDto
                {
                    Id = _cypherService.Encrypt(currentStep.Id),
                    Name = currentStep.Name,
                    Description = currentStep.Description
                },
                user = userDto,
                purchaseRequisition = purchaseRequisitionDto,
                organizations = organizations.Select(organization => new OrganizationDto
                {
                    PlainId = organization.Id,
                    Name = organization.Name,
                    Description = organization.Description
                }).ToList(),
                branches = branches.Select(branch => new BranchDto
                {
                    PlainId = branch.Id,
                    Name = branch.Name,
                    Description = branch.Description
                }).ToList(),
                departments = departments.Select(department => new DepartmentDto
                {
                    PlainId = department.Id,
                    Name = department.Name,
                    Description = department.Description
                }).ToList(),
                offices = offices.Select(office => new OfficeDto
                {
                    PlainId = office.Id,
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

    //submit-purchase-requisition-form
    [HttpPost("submit-purchase-requisition-form")]
    [HasPrivilege(PrivilegeConstant.SEC_EMPLOYEE)]
    public async Task<IActionResult> SubmitPurchaseRequisitionForm()
    {
        try
        {
            var purchaseRequisitionDto = new PurchaseRequisitionDto
            {
                DepartmentId = Request.Form["departmentId"].FirstOrDefault(),
                // ItemDescription = Request.Form["itemDescription"].FirstOrDefault(),
                // QuantityString = Request.Form["quantity"].FirstOrDefault(),
                // EstimatedCostString = Request.Form["estimatedCost"].FirstOrDefault(),
                Justification = Request.Form["justification"].FirstOrDefault()
            };

            var comment = Request.Form["comment"].FirstOrDefault();

            if (string.IsNullOrEmpty(comment))
                return Ok(new { success = false, message = "Comment is required" });

            if (string.IsNullOrEmpty(purchaseRequisitionDto.DepartmentId))
                return Ok(new { success = false, message = "Department is required" });

            // if (string.IsNullOrEmpty(purchaseRequisitionDto.ItemDescription))
            // return Ok(new { success = false, message = "Item description is required" });

            // if (string.IsNullOrEmpty(purchaseRequisitionDto.QuantityString))
            // return Ok(new { success = false, message = "Quantity is required" });

            // if (string.IsNullOrEmpty(purchaseRequisitionDto.EstimatedCostString))
            // return Ok(new { success = false, message = "Estimated cost is required" });

            if (string.IsNullOrEmpty(purchaseRequisitionDto.Justification))
                return Ok(new { success = false, message = "Justification is required" });

            var files = Request.Form.Files;
            var isValid = _systemAttachmentManager.IsValid(files);

            if (!isValid.Succeeded)
                return Ok(new { success = false, message = isValid.Errors.FirstOrDefault()?.Description });

            var uploadFiles = _systemAttachmentManager.UploadFiles(files, "purchase-requisition");

            if (!uploadFiles.Any())
                return Ok(new { success = false, message = "Unable to upload files. Please try again" });

            var currentDateTime = DateTimeOffset.UtcNow;
            var currentUserId = _userManager.GetUserId(User);

            var referenceNumber =
                await _workflowService.GenerateReferenceNumber(ModuleConstant.PROCURMENT.GetModuleCode()!);

            //create purchase requisition
            var purchaseRequisition = new PurchaseRequisition
            {
                RequestingOfficerUserId = currentUserId!,
                DepartmentId = purchaseRequisitionDto.DepartmentId,
                // ItemDescription = purchaseRequisitionDto.ItemDescription,
                // Quantity = Convert.ToInt32(purchaseRequisitionDto.QuantityString),
                // EstimatedCost = Convert.ToDecimal(purchaseRequisitionDto.EstimatedCostString),
                Justification = purchaseRequisitionDto.Justification,
                Reference = referenceNumber,
                CreatedByUserId = currentUserId!,
                CreatedDate = currentDateTime
            };

            await _purchaseRequisitionService.CreateAsync(purchaseRequisition);

            //create purchase requisition attachments
            var systemAttachments = uploadFiles.Select(dto => new SystemAttachment
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

            //initiate workflow task
            var workflowTask = await _workflowService.InitiateWorkflowTask(
                referenceNumber,
                currentUserId!,
                ProcessConstant.PROC_PURCHASE_REQUISITION_OF_SERVICES.GetProcessId()!,
                comment,
                systemAttachments
            );

            //update purchase requisition with workflow task
            if (workflowTask.task == null || !workflowTask.response.Succeeded)
                return Ok(new
                    { success = false, message = workflowTask.response.Errors.FirstOrDefault()?.Description });

            if (workflowTask is { task: not null, response.Succeeded: true })
                return Ok(new { success = true, message = workflowTask.response.SuccessDescription });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }

        return Ok(new { success = false, message = "An error occurred while processing the request" });
    }
}