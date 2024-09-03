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
[Route("api/security/department")]
public class DepartmentApiController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<DepartmentApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUtil _util;
    private readonly ICypherService _cypherService;

    public DepartmentApiController(IDatatableService datatableService,
        ILogger<DepartmentApiController> logger, UserManager<ApplicationUser> userManager, IBranchService branchService,
        IDepartmentService departmentService, IUtil util, ICypherService cypherService)
    {
        _datatableService = datatableService;
        _logger = logger;
        _userManager = userManager;
        _branchService = branchService;
        _departmentService = departmentService;
        _util = util;
        _cypherService = cypherService;
    }

    //count-offices-by-department-id
    [HttpPost("count-offices-by-department-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> CountOfficesByDepartmentId([FromForm] string departmentId)
    {
        try
        {
            departmentId = _cypherService.Decrypt(departmentId);
            var department = await _departmentService.FindByIdAsync(departmentId);
            if (department == null)
                return Ok(new { success = false, message = "Department not found" });

            var offices = await _departmentService.FindOfficesAsync(departmentId);
            var totalCount = offices.Count;
            return Ok(new { success = true, totalCount, departmentName = department.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //create
    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> CreateDepartment()
    {
        try
        {
            var departmentDto = new DepartmentDto
            {
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                BranchId = Request.Form["branchId"].FirstOrDefault()!
            };

            if (string.IsNullOrEmpty(departmentDto.Description))
                return Ok(new { success = false, message = "Department description is required" });

            if (string.IsNullOrEmpty(departmentDto.Name))
                return Ok(new { success = false, message = "Department name is required" });

            //trim and remove extra spaces
            departmentDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(departmentDto.Name);

            if (!string.IsNullOrEmpty(departmentDto.BranchId))
            {
                departmentDto.BranchId = _cypherService.Decrypt(departmentDto.BranchId);
                if (!await _branchService.BranchIdExistsAsync(departmentDto.BranchId))
                {
                    return Ok(new { success = false, message = "Selected branch not found" });
                }
            }

            if (await _departmentService.DepartmentNameExistsAsync(departmentDto.Name))
            {
                return Ok(new { success = false, message = "Department already exists" });
            }

            var currentUserId = _userManager.GetUserId(User);

            await _departmentService.CreateAsync(new Department
            {
                Name = departmentDto.Name!,
                Description = departmentDto.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = currentUserId!,
                BranchId = departmentDto.BranchId
            });
            return Ok(new { success = true, message = "Department created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //delete
    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> DeleteDepartment([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Department id is required" });

            id = _cypherService.Decrypt(id);
            var department = await _departmentService.FindByIdAsync(id);
            if (department == null)
                return Ok(new { success = false, message = "Department not found" });

            //check if department has offices
            var offices = await _departmentService.FindOfficesAsync(id);
            if (offices.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete department. It has " + offices.Count + " office(s)"
                });

            //delete department
            await _departmentService.DeleteAsync(department);
            return Ok(new { success = true, message = "Department deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //edit
    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> EditDepartment()
    {
        try
        {
            var departmentDto = new DepartmentDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                BranchId = Request.Form["branchId"].FirstOrDefault()!
            };

            if (string.IsNullOrEmpty(departmentDto.Description))
                return Ok(new { success = false, message = "Department description is required" });

            if (string.IsNullOrEmpty(departmentDto.Name))
                return Ok(new { success = false, message = "Department name is required" });

            if (departmentDto.Id == null)
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            //trim and remove extra spaces
            departmentDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(departmentDto.Name);

            if (!string.IsNullOrEmpty(departmentDto.BranchId))
            {
                departmentDto.BranchId = _cypherService.Decrypt(departmentDto.BranchId);
                if (!await _branchService.BranchIdExistsAsync(departmentDto.BranchId))
                {
                    return Ok(new { success = false, message = "Selected branch not found" });
                }
            }

            departmentDto.Id = _cypherService.Decrypt(departmentDto.Id!);
            if (await _departmentService.DepartmentNameExistsExceptAsync(departmentDto.Name, departmentDto.Id!))
            {
                return Ok(new { success = false, message = "Department already exists" });
            }

            var department = await _departmentService.FindByIdAsync(departmentDto.Id!);
            if (department == null)
                return Ok(new { success = false, message = "Department not found" });

            if (!string.IsNullOrEmpty(departmentDto.BranchId))
                department.BranchId = departmentDto.BranchId;

            department.Description = departmentDto.Description;
            department.Name = departmentDto.Name;
            department.ModifiedByUserId = _userManager.GetUserId(User);
            department.ModifiedDate = DateTimeOffset.UtcNow;
            await _departmentService.UpdateAsync(department);
            return Ok(new { success = true, message = "Department updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-departments-dt
    [HttpPost("get-departments-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> GetDepartmentsDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var departments = await _departmentService.FindAllAsync();

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
                    Description = department.Description,
                    Branch = new BranchDto
                    {
                        Id = _cypherService.Encrypt(department.Branch.Id),
                        Name = department.Branch.Name,
                        Description = department.Branch.Description
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

    //get-branches-select2
    [HttpPost("get-branches-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
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
                        Id = _cypherService.Encrypt(branch.Organization.Id),
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

    //get-department-data
    [HttpPost("get-department-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> GetDepartmentData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Department id is required" });

            id = _cypherService.Decrypt(id);
            var department = await _departmentService.FindByIdAsync(id);
            if (department == null)
                return Ok(new { success = false, message = "Department not found" });

            return Ok(new
            {
                success = true,
                department = new DepartmentDto
                {
                    Id = _cypherService.Encrypt(department.Id),
                    Name = department.Name,
                    Description = department.Description,
                    Branch = new BranchDto
                    {
                        Id = _cypherService.Encrypt(department.Branch.Id),
                        Name = department.Branch.Name,
                        Description = department.Branch.Description
                    }
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-offices-in-department-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_DEPARTMENTS)]
    public async Task<IActionResult> GetOfficesInDepartmentDt()
    {
        try
        {
            var departmentId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(departmentId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            departmentId = _cypherService.Decrypt(departmentId);
            var offices = await _departmentService.FindOfficesAsync(departmentId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                offices.ToList().Select((office, index) => new OfficeDto
                {
                    Id = _cypherService.Encrypt(office.Id),
                    Name = office.Name,
                    Counter = index + 1,
                    Description = office.Description
                    // Department = office.Department
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