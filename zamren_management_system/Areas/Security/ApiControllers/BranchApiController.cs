using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Area("Security")]
[Route("api/security/branch")]
public class BranchApiController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly IOrganizationService _organizationService;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<BranchApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUtil _util;
    private readonly ICypherService _cypherService;

    public BranchApiController(IDatatableService datatableService,
        ILogger<BranchApiController> logger, UserManager<ApplicationUser> userManager, IBranchService branchService,
        IOrganizationService organizationService, IUtil util,
        ICypherService cypherService)
    {
        _datatableService = datatableService;
        _logger = logger;
        _userManager = userManager;
        _branchService = branchService;
        _organizationService = organizationService;
        _util = util;
        _cypherService = cypherService;
    }

    [HttpPost("count-departments-by-branch-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> CountDepartmentsByBranchId([FromForm] string branchId)
    {
        try
        {
            if (string.IsNullOrEmpty(branchId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            branchId = _cypherService.Decrypt(branchId);
            var branch = await _branchService.FindByIdAsync(branchId);
            if (branch == null)
                return Ok(new { success = false, message = "Branch not found" });

            var departments = await _branchService.FindDepartmentsAsync(branchId);
            var totalCount = departments.Count;
            return Ok(new { success = true, totalCount, branchName = branch.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //create
    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> CreateBranch()
    {
        try
        {
            var branchDto = new BranchDto
            {
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                OrganizationId = Request.Form["organizationId"].FirstOrDefault()!
            };

            if (string.IsNullOrEmpty(branchDto.Description))
                return Ok(new { success = false, message = "Branch description is required" });

            if (string.IsNullOrEmpty(branchDto.Name))
                return Ok(new { success = false, message = "Branch name is required" });
            
            //trim and remove extra spaces
            branchDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(branchDto.Name);

            if (!string.IsNullOrEmpty(branchDto.OrganizationId))
            {
                branchDto.OrganizationId = _cypherService.Decrypt(branchDto.OrganizationId);
                if (!await _organizationService.OrganizationIdExistsAsync(branchDto.OrganizationId))
                {
                    return Ok(new { success = false, message = "Organization not found" });
                }
            }

            if (await _branchService.BranchNameExistsAsync(branchDto.Name))
            {
                return Ok(new { success = false, message = "Branch already exists" });
            }

            var currentUserId = _userManager.GetUserId(User);

            await _branchService.CreateAsync(new Branch
            {
                Name = branchDto.Name!,
                Description = branchDto.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                OrganizationId = branchDto.OrganizationId,
                CreatedByUserId = currentUserId!
            });
            return Ok(new { success = true, message = "Branch created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //delete
    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> DeleteBranch([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var branch = await _branchService.FindByIdAsync(id);
            if (branch == null)
                return Ok(new { success = false, message = "Branch not found" });

            //check if branch has departments
            var departments = await _branchService.FindDepartmentsAsync(id);
            if (departments.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete branch. It has " + departments.Count + " department(s)"
                });

            //delete branch
            await _branchService.DeleteAsync(branch);
            return Ok(new { success = true, message = "Branch deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //edit
    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> EditBranch()
    {
        try
        {
            var branchDto = new BranchDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                OrganizationId = Request.Form["organizationId"].FirstOrDefault()!
            };

            if (string.IsNullOrEmpty(branchDto.Description))
                return Ok(new { success = false, message = "Branch description is required" });

            if (string.IsNullOrEmpty(branchDto.Name))
                return Ok(new { success = false, message = "Branch name is required" });

            if (string.IsNullOrEmpty(branchDto.Id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            //trim and remove extra spaces
            branchDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(branchDto.Name);

            if (!string.IsNullOrEmpty(branchDto.OrganizationId))
            {
                branchDto.OrganizationId = _cypherService.Decrypt(branchDto.OrganizationId);
                if (!await _organizationService.OrganizationIdExistsAsync(branchDto.OrganizationId))
                {
                    return Ok(new { success = false, message = "Selected organization not found" });
                }
            }

            branchDto.Id = _cypherService.Decrypt(branchDto.Id!);
            if (await _branchService.BranchNameExistsExceptAsync(branchDto.Name, branchDto.Id!))
            {
                return Ok(new { success = false, message = "Branch already exists" });
            }

            var branch = await _branchService.FindByIdAsync(branchDto.Id!);
            if (branch == null)
                return Ok(new { success = false, message = "Branch not found" });

            if (!string.IsNullOrEmpty(branchDto.OrganizationId))
            {
                branch.OrganizationId = branchDto.OrganizationId;
            }

            branch.Description = branchDto.Description;
            branch.Name = branchDto.Name;
            branch.ModifiedByUserId = _userManager.GetUserId(User);
            branch.ModifiedDate = DateTimeOffset.UtcNow;
            await _branchService.UpdateAsync(branch);
            return Ok(new { success = true, message = "Branch updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-organizations-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> GetOrganizationsSelect2()
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

            var organizations = await _organizationService.FindAllAsync();

            // Filter the organizations based on the search term ignoring case
            var organizationsQuery = organizations
                .Where(organization => organization.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = organizationsQuery.Count;

            var organizationsList = organizationsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(organization => new OrganizationDto
                {
                    Id = _cypherService.Encrypt(organization.Id),
                    Name = organization.Name,
                    Description = organization.Description
                })
                .ToList();

            return Ok(new { results = organizationsList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-branches-except")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> GetBranchesExcept()
    {
        try
        {
            var branchId = Request.Form["branchId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(branchId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            branchId = _cypherService.Decrypt(branchId);
            var branches = await _branchService.FindAllExceptAsync(branchId);

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
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-branches-dt
    [HttpPost("get-branches-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> GetBranchesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var branches = await _branchService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                branches.ToList().Select((branch, index) => new BranchDto
                {
                    Id = _cypherService.Encrypt(branch.Id),
                    Name = branch.Name,
                    Counter = index + 1,
                    Description = branch.Description,
                    Organization = new OrganizationDto
                    {
                        Id = _cypherService.Encrypt(branch.OrganizationId),
                        Name = branch.Organization.Name,
                        Description = branch.Organization.Description
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

    //get-branch-data
    [HttpPost("get-branch-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> GetBranchData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            id = _cypherService.Decrypt(id);
            var branch = await _branchService.FindByIdAsync(id);
            if (branch == null)
                return Ok(new { success = false, message = "Branch not found" });

            return Ok(new
            {
                success = true,
                branch = new BranchDto
                {
                    Id = _cypherService.Encrypt(branch.Id),
                    Name = branch.Name,
                    Description = branch.Description,
                    Organization = new OrganizationDto
                    {
                        Id = _cypherService.Encrypt(branch.OrganizationId),
                        Name = branch.Organization.Name,
                        Description = branch.Organization.Description
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

    //get-departments-in-branch-dt2
    [HttpPost("get-departments-in-branch-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> GetDepartmentsInBranchDt2()
    {
        try
        {
            var branchId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(branchId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            branchId = _cypherService.Decrypt(branchId);
            var departments = await _branchService.FindDepartmentsAsync(branchId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                departments.ToList().Select((department, index) => new DepartmentDto
                {
                    Id = _cypherService.Encrypt(department.Id),
                    Name = department.Name,
                    Counter = index + 1,
                    Description = department.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-branches-select2
    [HttpPost("get-branches-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_BRANCHES)]
    public async Task<IActionResult> GetBranchesSelect2()
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

            var branches = await _branchService.FindAllAsync();

            // Filter the branches based on the search term ignoring case
            var branchesQuery = branches
                .Where(branch => branch.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = branchesQuery.Count;

            var branchesList = branchesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(branch => new BranchDto
                {
                    Id = _cypherService.Encrypt(branch.Id),
                    Name = branch.Name,
                    Description = branch.Description,
                    Organization = new OrganizationDto
                    {
                        Id = _cypherService.Encrypt(branch.OrganizationId),
                        Name = branch.Organization.Name,
                        Description = branch.Organization.Description
                    }
                })
                .ToList();

            return Ok(new { results = branchesList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}