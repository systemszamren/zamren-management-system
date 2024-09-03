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
[Route("api/security/organization")]
public class OrganizationApiController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<OrganizationApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUtil _util;
    private readonly ICypherService _cypherService;

    public OrganizationApiController(IDatatableService datatableService, IOrganizationService organizationService,
        ILogger<OrganizationApiController> logger, UserManager<ApplicationUser> userManager, IUtil util,
        ICypherService cypherService)
    {
        _datatableService = datatableService;
        _organizationService = organizationService;
        _logger = logger;
        _userManager = userManager;
        _util = util;
        _cypherService = cypherService;
    }

    [HttpPost("count-branches-by-organization-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> CountBranchesByOrganizationId([FromForm] string organizationId)
    {
        try
        {
            organizationId = _cypherService.Decrypt(organizationId);
            var organization = await _organizationService.FindByIdAsync(organizationId);
            if (organization == null)
                return Ok(new { success = false, message = "Organization not found" });

            var branches = await _organizationService.FindBranchesAsync(organizationId);
            var totalCount = branches.Count;
            return Ok(new { success = true, totalCount, organizationName = organization.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> CreateOrganization()
    {
        try
        {
            var organizationDto = new OrganizationDto
            {
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(organizationDto.Description))
                return Ok(new { success = false, message = "Organization description is required" });

            if (string.IsNullOrEmpty(organizationDto.Name))
                return Ok(new { success = false, message = "Organization name is required" });

            //trim and remove extra spaces
            organizationDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(organizationDto.Name);

            if (await _organizationService.OrganizationNameExistsAsync(organizationDto.Name))
            {
                return Ok(new { success = false, message = "Organization already exists" });
            }

            //get current user id
            var currentUserId = _userManager.GetUserId(User);

            await _organizationService.CreateAsync(new Organization
            {
                Name = organizationDto.Name,
                Description = organizationDto.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = currentUserId!
            });
            return Ok(new { success = true, message = "Organization created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> DeleteOrganization([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Organization is required" });

            id = _cypherService.Decrypt(id);
            var organization = await _organizationService.FindByIdAsync(id);
            if (organization == null)
                return Ok(new { success = false, message = "Organization not found" });

            //check if organization has branches
            var branches = await _organizationService.FindBranchesAsync(id);
            if (branches.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete organization. It has " + branches.Count + " branch(es)"
                });

            //delete organization
            await _organizationService.DeleteAsync(organization);
            return Ok(new { success = true, message = "Organization deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> EditOrganization()
    {
        try
        {
            var organizationDto = new OrganizationDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault()
            };

            //user string.IsNullOrEmpty instead of null check
            if (string.IsNullOrEmpty(organizationDto.Description))
                return Ok(new { success = false, message = "Organization description is required" });

            if (string.IsNullOrEmpty(organizationDto.Name))
                return Ok(new { success = false, message = "Organization name is required" });

            if (string.IsNullOrEmpty(organizationDto.Id))
                return Ok(new { success = false, message = "Organization is required" });

            //trim and remove extra spaces
            organizationDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(organizationDto.Name);

            organizationDto.Id = _cypherService.Decrypt(organizationDto.Id!);
            var organization = await _organizationService.FindByIdAsync(organizationDto.Id!);
            if (organization == null)
                return Ok(new { success = false, message = "Organization not found" });

            //check if organization exists
            if (await _organizationService.OrganizationNameExistsExceptAsync(organizationDto.Name, organizationDto.Id!))
                return Ok(new { success = false, message = "Organization already exists" });

            //update organization
            organization.Description = organizationDto.Description;
            organization.Name = organizationDto.Name;
            organization.ModifiedByUserId = _userManager.GetUserId(User);
            organization.ModifiedDate = DateTimeOffset.UtcNow;
            await _organizationService.UpdateAsync(organization);
            return Ok(new { success = true, message = "Organization updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-organizations
    [HttpPost("get-organizations-except")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> GetOrganizations()
    {
        try
        {
            var organizationId = Request.Form["organizationId"].FirstOrDefault() ?? "";
            
            if (string.IsNullOrEmpty(organizationId))
                return Ok(new { success = false, message = "Organization is required" });
            
            organizationId = _cypherService.Decrypt(organizationId);
            var organizations = await _organizationService.FindAllExceptAsync(organizationId);

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
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-organizations-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> GetOrganizationsDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var organizations = await _organizationService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                organizations.ToList().Select((organization, index) => new OrganizationDto
                {
                    Id = _cypherService.Encrypt(organization.Id),
                    Name = organization.Name,
                    Counter = index + 1,
                    Description = organization.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-branches-in-organization-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> GetBranchesInOrganizationDt()
    {
        try
        {
            var organizationId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            
            if (string.IsNullOrEmpty(organizationId))
                return Ok(new { success = false, message = "Organization is required" });

            organizationId = _cypherService.Decrypt(organizationId);
            var branches = await _organizationService.FindBranchesAsync(organizationId);

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
                    Description = branch.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-organization-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ORGANIZATIONS)]
    public async Task<IActionResult> GetOrganizationData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";
            
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Organization is required" });

            id = _cypherService.Decrypt(id);
            var organization = await _organizationService.FindByIdAsync(id);
            if (organization == null)
                return Ok(new { success = false, message = "Organization not found" });

            return Ok(new
            {
                success = true,
                organization = new OrganizationDto
                {
                    Id = _cypherService.Encrypt(organization.Id),
                    Name = organization.Name,
                    Description = organization.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}