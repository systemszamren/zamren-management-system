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
[Route("api/security/privilege")]
public class PrivilegeApiController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IPrivilegeService _privilegeService;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<PrivilegeApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUtil _util;
    private readonly ICypherService _cypherService;

    public PrivilegeApiController(RoleManager<ApplicationRole> roleManager, IPrivilegeService privilegeService,
        IDatatableService datatableService,
        ILogger<PrivilegeApiController> logger, UserManager<ApplicationUser> userManager, IUtil util,
        ICypherService cypherService)
    {
        _roleManager = roleManager;
        _privilegeService = privilegeService;
        _datatableService = datatableService;
        _logger = logger;
        _userManager = userManager;
        _util = util;
        _cypherService = cypherService;
    }

    [HttpPost("get-privileges-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> GetPrivilegesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var privileges = await _privilegeService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                privileges.ToList().Select((privilege, index) => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege.Id),
                    Name = privilege.Name,
                    Counter = index + 1,
                    Description = privilege.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //create
    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> CreatePrivilege()
    {
        try
        {
            var privilegeDto = new PrivilegeDto
            {
                Description = Request.Form["description"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(privilegeDto.Description))
                return Ok(new { success = false, message = "Privilege description is required" });

            if (string.IsNullOrEmpty(privilegeDto.Name))
                return Ok(new { success = false, message = "Privilege name is required" });

            //trim and remove extra spaces
            privilegeDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(privilegeDto.Name);

            if (privilegeDto.Name.Contains(' '))
                privilegeDto.Name = privilegeDto.Name.Replace(" ", "_");

            if (await _privilegeService.PrivilegeNameExistsAsync(privilegeDto.Name))
            {
                return Ok(new { success = false, message = "Privilege already exists" });
            }

            //get current user id
            var currentUserId = _userManager.GetUserId(User);

            var privilege = new Privilege
            {
                Name = privilegeDto.Name!,
                Description = privilegeDto.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = currentUserId!
            };

            await _privilegeService.CreateAsync(privilege);
            return Ok(new { success = true, message = "Privilege created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete-privilege")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> DeletePrivilege([FromForm] string id)
    {
        try
        {
            id = _cypherService.Decrypt(id);
            var privilege = await _privilegeService.FindByIdAsync(id);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            //check if privilege is assigned to any role
            var rolesWithPrivilege = await _privilegeService.FindRolesAsync(id);
            var roles = rolesWithPrivilege.ToList();
            if (roles.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete privilege. It is assigned to " + roles.Count + " role(s)"
                });

            //delete privilege
            await _privilegeService.DeleteAsync(privilege);
            return Ok(new { success = true, message = "Privilege deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //add privilege to role
    [HttpPost("add-to-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> AddPrivilegeToRole()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault();
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault();

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "Role is required" });

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "Privilege is required" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return Ok(new { success = false, message = "Role not found" });
            }

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
            {
                return Ok(new { success = false, message = "Privilege not found" });
            }

            //add privilege to role
            var currentUserId = _userManager.GetUserId(User);
            await _privilegeService.AddToRoleAsync(new RolePrivilege
            {
                RoleId = roleId,
                PrivilegeId = privilegeId,
                CreatedByUserId = currentUserId!,
                CreatedDate = DateTimeOffset.UtcNow
            });
            return Ok(new { success = true, message = "Privilege added to role successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("remove-from-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> RemovePrivilegeFromRole()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault();
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault();

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "Role is required" });

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "Privilege is required" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return Ok(new { success = false, message = "Role not found" });
            }

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
            {
                return Ok(new { success = false, message = "Privilege not found" });
            }

            //remove privilege from role
            await _privilegeService.RemoveFromRoleAsync(roleId, privilegeId);
            return Ok(new { success = true, message = "Privilege removed from role successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-privilege-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> GetPrivilegeData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";
            
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Privilege id is required" });

            id = _cypherService.Decrypt(id);
            var privilege = await _privilegeService.FindByIdAsync(id);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            return Ok(new
            {
                success = true,
                privilege = new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege.Id),
                    Name = privilege.Name,
                    Description = privilege.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> EditPrivilege()
    {
        try
        {
            var privilegeDto = new PrivilegeDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault()
            };

            //user string.IsNullOrEmpty instead of null check
            if (string.IsNullOrEmpty(privilegeDto.Description))
                return Ok(new { success = false, message = "Privilege description is required" });

            if (string.IsNullOrEmpty(privilegeDto.Name))
                return Ok(new { success = false, message = "Privilege name is required" });
            
            if (string.IsNullOrEmpty(privilegeDto.Id))
                return Ok(new { success = false, message = "Privilege id is required" });

            //trim and remove extra spaces
            privilegeDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(privilegeDto.Name);

            //Name: replace white space with underscore
            if (privilegeDto.Name.Contains(' '))
                privilegeDto.Name = privilegeDto.Name.Replace(" ", "_");

            privilegeDto.Id = _cypherService.Decrypt(privilegeDto.Id!);
            var privilege = await _privilegeService.FindByIdAsync(privilegeDto.Id!);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            //check if privilege name exists
            if (await _privilegeService.PrivilegeNameExistsExceptAsync(privilegeDto.Name, privilegeDto.Id!))
            {
                return Ok(new { success = false, message = "Privilege already exists" });
            }

            //update privilege
            privilege.Description = privilegeDto.Description;
            privilege.Name = privilegeDto.Name;
            privilege.ModifiedByUserId = _userManager.GetUserId(User);
            privilege.ModifiedDate = DateTimeOffset.UtcNow;
            await _privilegeService.UpdateAsync(privilege);
            return Ok(new { success = true, message = "Privilege updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("count-roles-by-privilege-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> CountRolesByPrivilegeId([FromForm] string privilegeId)
    {
        try
        {
            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            var rolesWithPrivilege = await _privilegeService.FindRolesAsync(privilegeId);
            var totalCount = rolesWithPrivilege.Count();
            return Ok(new { success = true, totalCount, privilegeName = privilege.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}