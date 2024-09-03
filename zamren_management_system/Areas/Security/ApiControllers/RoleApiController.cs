using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
[Route("api/security/role")]
public class RoleApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<RoleApiController> _logger;
    private readonly IPrivilegeService _privilegeService;
    private readonly IUserRoleService _userRoleService;
    private readonly IConfiguration _configuration;
    private readonly IUtil _util;
    private readonly ICypherService _cypherService;

    public RoleApiController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        IDatatableService datatableService, ILogger<RoleApiController> logger, IPrivilegeService privilegeService,
        IUserRoleService userRoleService, IUtil util, IConfiguration configuration, ICypherService cypherService,
        IUserService userService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _datatableService = datatableService;
        _logger = logger;
        _privilegeService = privilegeService;
        _userRoleService = userRoleService;
        _util = util;
        _configuration = configuration;
        _cypherService = cypherService;
        _userService = userService;
    }

    [HttpPost("count-users-by-role-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> CountUsersByRoleId([FromForm] string roleId)
    {
        try
        {
            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            var totalCount = usersInRole.Count;
            return Ok(new { success = true, totalCount, roleName = role.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> CreateRole()
    {
        try
        {
            var roleDto = new RoleDto
            {
                Description = Request.Form["description"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault()
            };

            //change null check to string.IsNullOrEmpty
            if (string.IsNullOrEmpty(roleDto.Description))
                return Ok(new { success = false, message = "Role description is required" });

            if (string.IsNullOrEmpty(roleDto.Name))
                return Ok(new { success = false, message = "Role name is required" });

            //trim and remove extra spaces
            roleDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(roleDto.Name);

            if (await _roleManager.RoleExistsAsync(roleDto.Name))
            {
                return Ok(new { success = false, message = "Role already exists" });
            }

            var role = new ApplicationRole
            {
                Name = roleDto.Name!,
                Description = roleDto.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = _userManager.GetUserId(User)!
            };

            await _roleManager.CreateAsync(role);
            return Ok(new { success = true, message = "Role created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> DeleteRole([FromForm] string roleId)
    {
        try
        {
            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //check if role has users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

            if (usersInRole.Count > 0)
                return Ok(new
                    { success = false, message = "Role has " + usersInRole.Count + " user(s). Cannot delete role" });

            //check if role has privileges
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            if (roleClaims.Count > 0)
                return Ok(new
                {
                    success = false, message = "Role has " + roleClaims.Count + " privilege(s). Cannot delete role"
                });

            //delete role
            var result = await _roleManager.DeleteAsync(role);
            return Ok(result.Succeeded
                ? new { success = true, message = "Role deleted successfully" }
                : new { success = false, message = "Role deletion failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    /**
     * Get all roles for the datatable
     */
    [HttpPost("get-roles-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public IActionResult GetRolesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var roles = _roleManager.Roles;

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                role => role.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                role => role,
                roles.ToList().Select((role, index) => new RoleDto
                {
                    Id = _cypherService.Encrypt(role.Id),
                    Counter = index + 1,
                    Name = role.Name!,
                    Description = role.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-privileges-in-role-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetPrivilegesInRoleDt()
    {
        try
        {
            var roleId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var privileges = await _privilegeService.FindInRoleAsync(roleId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                privileges.ToList().Select((privilege, index) => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege!.Id),
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

    [HttpPost("get-users-in-role-dt1")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetUsersInRoleDt1()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            var roleId = Request.Form["id"].FirstOrDefault();

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "Role Id not provided" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            // var userRoleDto = await _userManager.GetUsersInRoleAsync(role.Name!);
            var userRoleDto = await _userRoleService.GetRoleUsers(role);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                userDto => userDto.FirstName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                           || userDto.LastName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                           || (userDto.Email != null &&
                               userDto.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),
                userDto => userDto,
                userRoleDto.ToList().Select((userRole, index) => new UserRoleDto
                {
                    Counter = index + 1,
                    UserId = _cypherService.Encrypt(userRole.UserId!),
                    FirstName = userRole.FirstName,
                    LastName = userRole.LastName,
                    Email = userRole.Email,
                    StartDate = userRole.StartDate.ToLocalTime(),
                    EndDate = userRole.EndDate.ToLocalTime(),
                    IsActive = userRole.EndDate < DateTimeOffset.UtcNow
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-users-in-role-dt2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetUsersInRoleDt2()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            var roleId = Request.Form["id"].FirstOrDefault();

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "Role Id not provided" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            var userRoleDto = await _userRoleService.GetRoleUsers(role);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                userDto => userDto.FullName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                           || (userDto.Email != null &&
                               userDto.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),
                userDto => userDto,
                userRoleDto.ToList().Select((userRole, index) => new UserRoleDto
                {
                    Counter = index + 1,
                    UniqueId = _cypherService.Encrypt(userRole.UniqueId!),
                    UserId = _cypherService.Encrypt(userRole.UserId!),
                    RoleId = _cypherService.Encrypt(userRole.RoleId!),
                    // FirstName = userRole.FirstName,
                    // LastName = userRole.LastName,
                    FullName = userRole.FirstName + " " + userRole.LastName,
                    Email = userRole.Email,
                    RoleName = userRole.RoleName!,
                    StartDate = userRole.StartDate.ToLocalTime(),
                    EndDate = userRole.EndDate.ToLocalTime()
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-privileges-in-role-dt2
    [HttpPost("get-privileges-in-role-dt2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetPrivilegesInRoleDt2()
    {
        try
        {
            var roleId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var privileges = await _privilegeService.FindInRoleAsync(roleId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                privileges.ToList().Select((privilege, index) => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege!.Id),
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

    [HttpPost("get-users-not-in-role-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetUsersNotInRoleSelect2()
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
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //get all users
            var allUsers = await _userManager.Users.ToListAsync();
            // var allUsers = await _userService.FindAllEmployeesAsync();

            //get all users not in the role by role id
            var usersNotInRole = new List<ApplicationUser>();
            foreach (var user in allUsers)
            {
                if (!await _userManager.IsInRoleAsync(user, role.Name!))
                {
                    usersNotInRole.Add(user);
                }
            }

            // Filter the users based on the search term
            var usersQuery = usersNotInRole
                .Where(user =>
                    user.Email != null && (user.Email.Contains(q) || user.FirstName.Contains(q) ||
                                           user.LastName.Contains(q))).ToList();

            var totalCount = usersQuery.Count;

            var users = usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new UserDto
                {
                    Id = _cypherService.Encrypt(user.Id),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                })
                .ToList();

            return Ok(new { results = users, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-privileges-not-in-role-select2
    [HttpPost("get-privileges-not-in-role-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetPrivilegesNotInRoleSelect2()
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
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //get all privileges not in the role by role id
            var allPrivileges = await _privilegeService.FindNotInRoleAsync(roleId);

            // Filter the privileges based on the search term ignoring case
            var privilegesQuery = allPrivileges
                .Where(privilege => privilege!.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = privilegesQuery.Count;

            var privileges = privilegesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(privilege => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege!.Id),
                    Name = privilege.Name,
                    Description = privilege.Description
                })
                .ToList();

            return Ok(new { results = privileges, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }


    [HttpPost("get-roles-not-assigned-to-user-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetRolesNotAssignedToUserSelect2()
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
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            //get all roles
            var allRoles = await _roleManager.Roles.ToListAsync();

            //get all roles not assigned to the user by user id
            var rolesNotAssignedToUser = new List<ApplicationRole>();
            foreach (var role in allRoles)
            {
                if (!await _userManager.IsInRoleAsync(user, role.Name!))
                {
                    rolesNotAssignedToUser.Add(role);
                }
            }

            // Filter the roles based on the search term ignoring case
            var rolesQuery = rolesNotAssignedToUser
                .Where(role => role.Name!.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = rolesQuery.Count;

            var roles = rolesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(role => new RoleDto
                {
                    Id = _cypherService.Encrypt(role.Id),
                    Name = role.Name!,
                    Description = role.Description
                })
                .ToList();

            return Ok(new { results = roles, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //check-role-assigned-to-user
    [HttpPost("check-role-assigned-to-user")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> CheckRoleAssignedToUser()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //check if role is assigned to user
            var roleAssignedToUser = await _userManager.IsInRoleAsync(user, role.Name!);
            return Ok(new { success = true, assigned = roleAssignedToUser });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("check-user-exists-in-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> CheckUserExistsInRole()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //check if user is in role
            var userExistsInRole = await _userManager.IsInRoleAsync(user, role.Name!);
            return Ok(new { success = true, exists = userExistsInRole });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("check-privilege-exists-in-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> CheckPrivilegeExistsInRole()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            //check if privilege is in role
            var privilegeExistsInRole = await _privilegeService.IsInRoleAsync(roleId, privilegeId);
            return Ok(new { success = true, exists = privilegeExistsInRole });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("remove-user-from-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> RemoveUserFromRole()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //remove user from role
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
            return Ok(result.Succeeded
                ? new
                {
                    success = true, message = user.FirstName + " " + user.LastName + " removed from role successfully"
                }
                : new { success = false, message = "Unable to remove user from role" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("remove-privilege-from-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> RemovePrivilegeFromRole()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            await _privilegeService.RemoveFromRoleAsync(roleId, privilegeId);
            return Ok(new { success = true, message = "Privilege removed from role successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("remove-role-from-user")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> RemoveRoleFromUser()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
            if (role.Name == clientRoleName)
                return Ok(new { success = false, message = "Cannot remove Client role from user" });

            var adminRoleName = _configuration["DefaultUserRoles:AdminRole"] ?? string.Empty;
            if (role.Name == adminRoleName)
            {
                //check if user is the only admin
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                if (usersInRole.Count == 1)
                    return Ok(new { success = false, message = "Cannot remove the only admin from the system" });

                var adminUserId = _configuration["SystemAdminAccount:UserId"] ?? string.Empty;
                if (user.Id == adminUserId)
                    return Ok(new { success = false, message = "Cannot remove the admin role from the system admin" });
            }

            //remove user from role
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
            return Ok(result.Succeeded
                ? new
                {
                    success = true, message = role.Name + " removed from user successfully"
                }
                : new { success = false, message = "Unable to remove role from user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("add-user-to-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> AddUserToRole()
    {
        try
        {
            var roleId = Request.Form["currentRoleId"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var startDate = Request.Form["startDate"].FirstOrDefault() ?? "";
            var endDate = Request.Form["endDate"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "User is required" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "Role is required" });

            if (string.IsNullOrEmpty(startDate))
                return Ok(new { success = false, message = "Start date is required" });

            if (string.IsNullOrEmpty(endDate))
                return Ok(new { success = false, message = "End date is required" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //if end date is before start date
            if (_util.ConvertStringToDateTimeOffset(startDate) > _util.ConvertStringToDateTimeOffset(endDate, true))
                return Ok(new { success = false, message = "End date cannot be before start date" });

            var userInRole = await _userManager.IsInRoleAsync(user, role.Name!);
            if (userInRole)
                return Ok(new { success = false, message = "User already assigned to role" });

            // var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
            // var adminRoleName = _configuration["DefaultUserRoles:AdminRole"] ?? string.Empty;

            // Check if the user is an employee
            /*if (user.IsEmployee)
            {
                // If the role being assigned is the client role, prevent assignment
                if (role.Name == clientRoleName)
                {
                    //check if user has admin role
                    if (!await _userManager.IsInRoleAsync(user, adminRoleName))
                    {
                        return Ok(new { success = false, message = "Employees cannot be assigned the client role" });
                    }
                }
            }*/

            //none employees should not be assigned the other roles except the client role
            /*if (!user.IsEmployee && role.Name != clientRoleName)
            {
                return Ok(new
                {
                    success = false,
                    message = "This user cannot be assigned this role, please assign them a client role"
                });
            }*/

            var result = await _userRoleService.CreateAsync(new ApplicationUserRole
            {
                RoleId = role.Id,
                UserId = user.Id,
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTimeOffset.UtcNow,
                StartDate = _util.ConvertStringToDateTimeOffset(startDate),
                EndDate = _util.ConvertStringToDateTimeOffset(endDate, true)
            });

            return Ok(result.Succeeded
                ? new { success = true, message = "User added to role successfully" }
                : new { success = false, message = "Unable to add user to role" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("add-privilege-to-role")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_PRIVILEGES)]
    public async Task<IActionResult> AddPrivilegeToRole()
    {
        try
        {
            var roleId = Request.Form["currentRoleId"].FirstOrDefault() ?? "";
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            var result = await _privilegeService.AddToRoleAsync(new RolePrivilege
            {
                RoleId = role.Id,
                PrivilegeId = privilege.Id,
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTimeOffset.UtcNow
            });

            return Ok(result.Succeeded
                ? new { success = true, message = "Privilege added to role successfully" }
                : new { success = false, message = "Unable to add privilege to role" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("add-role-to-user")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> AddRoleToUser()
    {
        try
        {
            var roleId = Request.Form["roleId"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var startDate = Request.Form["startDate"].FirstOrDefault() ?? "";
            var endDate = Request.Form["endDate"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "User is required" });

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "Role is required" });

            if (string.IsNullOrEmpty(startDate))
                return Ok(new { success = false, message = "Start date is required" });

            if (string.IsNullOrEmpty(endDate))
                return Ok(new { success = false, message = "End date is required" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            if (string.IsNullOrEmpty(startDate))
                return Ok(new { success = false, message = "Start date is required" });

            if (string.IsNullOrEmpty(endDate))
                return Ok(new { success = false, message = "End date is required" });

            //if end date is before start date
            if (_util.ConvertStringToDateTimeOffset(startDate) > _util.ConvertStringToDateTimeOffset(endDate, true))
                return Ok(new { success = false, message = "End date cannot be before start date" });

            //check if role is 'Admin'
            var adminRoleName = _configuration["DefaultUserRoles:AdminRole"] ?? string.Empty;
            if (role.Name == adminRoleName)
            {
                //check if user is the only admin
                var adminUsers = await _userManager.GetUsersInRoleAsync(role.Name);
                if (adminUsers.Count == 1)
                    return Ok(new
                        { success = false, message = "Cannot add another system administrator to the system" });
            }

            var userInRole = await _userManager.IsInRoleAsync(user, role.Name!);
            if (userInRole)
                return Ok(new { success = false, message = "Role already assigned to user" });

            var result = await _userRoleService.CreateAsync(new ApplicationUserRole
            {
                RoleId = role.Id,
                UserId = user.Id,
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTimeOffset.UtcNow,
                StartDate = _util.ConvertStringToDateTimeOffset(startDate),
                EndDate = _util.ConvertStringToDateTimeOffset(endDate, true)
            });

            return Ok(result.Succeeded
                ? new { success = true, message = "Role added to user successfully" }
                : new { success = false, message = "Unable to add role to user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    /**
     * Get user's roles for the datatable
     */
    [HttpPost("get-user-roles-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetUserRolesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            var userRoles = await _userRoleService.GetUserRoles(user.Id);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                userRoleDto => userRoleDto.RoleName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                               userRoleDto.RoleDescription!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                role => role,
                userRoles.ToList().Select((role, index) => new UserRoleDto
                {
                    Counter = index + 1,
                    UniqueId = _cypherService.Encrypt(role.UniqueId!),
                    RoleId = _cypherService.Encrypt(role.RoleId!),
                    UserId = _cypherService.Encrypt(role.UserId!),
                    RoleName = role.RoleName!,
                    RoleDescription = role.RoleDescription,
                    StartDate = role.StartDate.ToLocalTime(),
                    EndDate = role.EndDate.ToLocalTime()
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-role-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetRoleData()
    {
        try
        {
            var roleId = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(roleId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            roleId = _cypherService.Decrypt(roleId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            return Ok(new
            {
                success = true,
                role = new RoleDto
                {
                    Id = _cypherService.Encrypt(role.Id),
                    Name = role.Name!,
                    Description = role.Description
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
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> EditRole()
    {
        try
        {
            var roleDto = new RoleDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(roleDto.Description))
                return Ok(new { success = false, message = "Role description is required" });

            if (string.IsNullOrEmpty(roleDto.Name))
                return Ok(new { success = false, message = "Role name is required" });

            if (string.IsNullOrEmpty(roleDto.Id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            //trim and remove extra spaces
            roleDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(roleDto.Name);

            roleDto.Id = _cypherService.Decrypt(roleDto.Id!);
            var role = await _roleManager.FindByIdAsync(roleDto.Id!);
            if (role == null)
                return Ok(new { success = false, message = "Role not found" });

            //check if role is 'Client'
            var clientRoleName = _configuration["DefaultUserRoles:ClientRole"] ?? string.Empty;
            if (role.Name == clientRoleName)
                return Ok(new { success = false, message = "Cannot edit Client role" });

            //check if role is admin
            var adminRoleName = _configuration["DefaultUserRoles:AdminRole"] ?? string.Empty;
            if (role.Name == adminRoleName)
                return Ok(new { success = false, message = "Cannot edit Admin role" });

            //check if role name exists except the current role
            if (await _roleManager.RoleExistsAsync(roleDto.Name) && role.Name != roleDto.Name)
            {
                return Ok(new { success = false, message = "Role already exists" });
            }

            //update role
            role.Description = roleDto.Description;
            role.Name = roleDto.Name;
            role.ModifiedByUserId = _userManager.GetUserId(User);
            role.ModifiedDate = DateTimeOffset.UtcNow;
            await _roleManager.UpdateAsync(role);
            return Ok(new { success = true, message = "Role updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-role-user-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> GetRoleUserData()
    {
        try
        {
            var uniqueId = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(uniqueId))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            uniqueId = _cypherService.Decrypt(uniqueId);
            var userRole = await _userRoleService.FindByIdAsync(uniqueId);

            if (userRole == null)
                return Ok(new { success = false, message = "User role not found" });

            return Ok(new
            {
                success = true, userRole = new UserRoleDto
                {
                    UniqueId = _cypherService.Encrypt(userRole.UniqueId),
                    UserId = _cypherService.Encrypt(userRole.UserId),
                    RoleId = _cypherService.Encrypt(userRole.RoleId),
                    StartDate = userRole.StartDate.ToLocalTime(),
                    EndDate = userRole.EndDate.ToLocalTime(),
                    FirstName = userRole.User.FirstName,
                    LastName = userRole.User.LastName,
                    Email = userRole.User.Email,
                    RoleName = userRole.Role.Name
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit-user-role-tenure")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_ROLES)]
    public async Task<IActionResult> EditUserRoleTenure()
    {
        try
        {
            var userRoleDto = new UserRoleDto
            {
                UniqueId = Request.Form["id"].FirstOrDefault(),
                StartDateString = Request.Form["startDate"].FirstOrDefault(),
                EndDateString = Request.Form["endDate"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(userRoleDto.UniqueId))
                return Ok(new { success = false, message = "User role not found" });

            if (string.IsNullOrEmpty(userRoleDto.StartDateString))
                return Ok(new { success = false, message = "Start date is required" });

            if (string.IsNullOrEmpty(userRoleDto.EndDateString))
                return Ok(new { success = false, message = "End date is required" });

            //if end date is before start date
            if (_util.ConvertStringToDateTimeOffset(userRoleDto.StartDateString) >
                _util.ConvertStringToDateTimeOffset(userRoleDto.EndDateString, true))
                return Ok(new { success = false, message = "End date cannot be before start date" });

            userRoleDto.UniqueId = _cypherService.Decrypt(userRoleDto.UniqueId!);
            var userRole = await _userRoleService.FindByIdAsync(userRoleDto.UniqueId!);
            if (userRole == null)
                return Ok(new { success = false, message = "User role not found" });

            userRole.StartDate = _util.ConvertStringToDateTimeOffset(userRoleDto.StartDateString);
            userRole.EndDate = _util.ConvertStringToDateTimeOffset(userRoleDto.EndDateString, true);
            userRole.ModifiedDate = DateTimeOffset.UtcNow;
            userRole.ModifiedByUserId = _userManager.GetUserId(User)!;
            await _userRoleService.UpdateAsync(userRole);
            return Ok(new
            {
                success = true, roleId = _cypherService.Encrypt(userRole.RoleId),
                message = "User role tenure updated successfully"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}