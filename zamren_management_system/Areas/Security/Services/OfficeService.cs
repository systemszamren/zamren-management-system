using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class OfficeService : IOfficeService
{
    private readonly AuthContext _context;
    private readonly ILogger<OfficeService> _logger;

    public OfficeService(AuthContext context, ILogger<OfficeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(Office office)
    {
        try
        {
            await _context.Offices.AddAsync(office);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(Office office)
    {
        try
        {
            _context.Offices.Update(office);
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
    
    //UpdateUserOfficeAsync
    public async Task<IdentityResult> UpdateUserOfficeAsync(UserOffice userOffice)
    {
        try
        {
            _context.UserOffices.Update(userOffice);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateUserOfficeAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<Office?> FindByIdAsync(string officeId)
    {
        return await _context.Offices
            .Include(o => o.Department)
            .FirstOrDefaultAsync(o => o.Id == officeId);
    }

    public async Task<List<Office>> FindAllAsync()
    {
        //include departments
        return await _context.Offices
            .Include(o => o.Department)
            .ToListAsync();
    }

    public async Task<IdentityResult> DeleteAsync(Office office)
    {
        try
        {
            _context.Offices.Remove(office);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    //FindUserOfficeByIdAsync
    
    public async Task<List<UserOffice>> FindUsersAsync(string officeId)
    {
        return await _context.UserOffices
            .Where(uo => uo.OfficeId == officeId)
            .Include(uo => uo.User)
            .Include(uo => uo.Office)
            .ToListAsync();
    }

    public async Task<IdentityResult> AddUserAsync(UserOffice userOffice)
    {
        try
        {
            await _context.UserOffices.AddAsync(userOffice);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "AddUserAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<bool> OfficeNameExistsAsync(string name)
    {
        return await _context.Offices.AnyAsync(o => o.Name == name);
    }

    //Checks if record exists except for the record with the given ID
    public async Task<bool> OfficeNameExistsExceptAsync(string name, string id)
    {
        return await _context.Offices.AnyAsync(o => o.Name == name && o.Id != id);
    }

    public async Task<bool> IsInOfficeAsync(string userId, string officeId)
    {
        return await _context.UserOffices.AnyAsync(uo => uo.UserId == userId && uo.OfficeId == officeId);
    }

    public async Task<ICollection<ApplicationUser>> FindUsersNotInOfficeAsync(string officeId)
    {
        return await _context.Users
            .Where(u => !_context.UserOffices.Any(uo => uo.UserId == u.Id && uo.OfficeId == officeId))
            .ToListAsync();
    }
    
    //FindUserOfficeByIdAsync
    public async Task<UserOffice?> FindUserOfficeByIdAsync(string id)
    {
        return await _context.UserOffices
            .Include(uo => uo.User)
            .Include(uo => uo.Office)
            .FirstOrDefaultAsync(uo => uo.Id == id);
    }

    public async Task<IdentityResult> RemoveUserAsync(UserOffice userOffice)
    {
        try
        {
            _context.UserOffices.Remove(userOffice);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RemoveUserAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }
    
    //FindUserOfficeAsync
    public async Task<UserOffice?> GetUserOfficeAsync(string userId, string officeId)
    {
        return await _context.UserOffices
            .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OfficeId == officeId);
    }
    
    //Find User's Office, Department, Branch, and Organization by User ID
    public async Task<UserOffice?> FindOfficeByUserIdAsync(string userId)
    {
        return await _context.UserOffices
            .Include(uo => uo.Office)
            .ThenInclude(o => o.Department)
            .ThenInclude(d => d.Branch)
            .ThenInclude(b => b.Organization)
            .FirstOrDefaultAsync(uo => uo.UserId == userId);
    }

    public async Task<List<Office>> FindAllExceptAsync(string officeId)
    {
        return await _context.Offices.Where(o => o.Id != officeId).ToListAsync();
    }
    
    //FindByDepartmentIdAsync
    public async Task<List<Office>> FindByDepartmentIdAsync(string departmentId)
    {
        return await _context.Offices.Where(o => o.DepartmentId == departmentId).ToListAsync();
    }
}