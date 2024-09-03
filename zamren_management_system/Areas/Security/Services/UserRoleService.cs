using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Security.Services;

public class UserRoleService : IUserRoleService
{
    private readonly AuthContext _context;
    private readonly ILogger<UserRoleService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRoleService(AuthContext context, ILogger<UserRoleService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUserRole userRole)
    {
        try
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "AddRoleToUserAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<ApplicationUserRole?> FindByUserIdAndRoleIdAsync(string userId, string roleId)
    {
        return await _context.UserRoles.FindAsync(userId, roleId);
    }

    public async Task<List<UserRoleDto>> GetRoleUsers(ApplicationRole role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role.Name!);

        var userRoleDto = new List<UserRoleDto>();
        foreach (var user in users)
        {
            var userRole = await FindByUserIdAndRoleIdAsync(user.Id, role.Id);
            if (userRole == null)
                continue;

            userRoleDto.Add(new UserRoleDto
            {
                UniqueId = userRole.UniqueId,
                UserId = user.Id,
                RoleId = role.Id,
                RoleName = role.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                StartDate = userRole.StartDate,
                EndDate = userRole.EndDate
            });
        }

        return userRoleDto;
    }

    public async Task<List<ApplicationUser>> GetRoleUsers2(ApplicationRole role)
    {
        return (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync(role.Name!);
    }

    //GetUserRoles
    public async Task<List<UserRoleDto>> GetUserRoles(string userId)
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        return userRoles.Select(userRole => new UserRoleDto
            {
                UniqueId = userRole.UniqueId,
                UserId = userRole.UserId,
                RoleId = userRole.RoleId,
                RoleName = userRole.Role.Name,
                RoleDescription = userRole.Role.Description,
                FirstName = userRole.User.FirstName,
                LastName = userRole.User.LastName,
                Email = userRole.User.Email,
                StartDate = userRole.StartDate,
                EndDate = userRole.EndDate
            })
            .ToList();
    }


    //FindByIdAsync
    public async Task<ApplicationUserRole?> FindByIdAsync(string uniqueId)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UniqueId == uniqueId);
    }

    //UpdateAsync
    public async Task<IdentityResult> UpdateAsync(ApplicationUserRole userRole)
    {
        try
        {
            _context.UserRoles.Update(userRole);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }
}