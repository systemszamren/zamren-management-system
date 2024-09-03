using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AuthContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(AuthContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(Department department)
    {
        try
        {
            await _context.Departments.AddAsync(department);
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

    public async Task<IdentityResult> UpdateAsync(Department department)
    {
        try
        {
            _context.Departments.Update(department);
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

    public async Task<IdentityResult> DeleteAsync(Department department)
    {
        try
        {
            _context.Departments.Remove(department);
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

    public async Task<Department?> FindByIdAsync(string departmentId)
    {
        return await _context.Departments
            .Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == departmentId);
    }

    public async Task<List<Department>> FindAllAsync()
    {
        return await _context.Departments
            .Include(d => d.Branch)
            .ToListAsync();
    }

    public async Task<ICollection<Office>> FindOfficesAsync(string departmentId)
    {
        return await _context.Offices.Where(o => o.DepartmentId == departmentId).ToListAsync();
    }

    public async Task<ICollection<ApplicationUser>> FindUsersAsync(string departmentId)
    {
        var offices = await FindOfficesAsync(departmentId);
        var users = new List<ApplicationUser>();
        foreach (var officeId in offices)
        {
            var officeUsers = await _context.UserOffices
                .Where(uo => uo.OfficeId == officeId.Id)
                .Select(uo => uo.User)
                .ToListAsync();
            users.AddRange(officeUsers);
        }

        return users;
    }

    public async Task<bool> DepartmentNameExistsAsync(string name)
    {
        return await _context.Departments.AnyAsync(d => d.Name == name);
    }

    //Checks if record exists except for the record with the given ID
    public async Task<bool> DepartmentNameExistsExceptAsync(string name, string id)
    {
        return await _context.Departments.AnyAsync(d => d.Name == name && d.Id != id);
    }

    public Task<bool> DepartmentIdExistsAsync(string departmentId)
    {
        return _context.Departments.AnyAsync(d => d.Id == departmentId);
    }

    //FindAllExceptAsync
    public async Task<List<Department>> FindAllExceptAsync(string departmentId)
    {
        return await _context.Departments
            .Where(d => d.Id != departmentId)
            .ToListAsync();
    }
    
    //FindByBranchIdAsync
    public async Task<ICollection<Department>> FindByBranchIdAsync(string branchId)
    {
        return await _context.Departments
            .Where(d => d.BranchId == branchId)
            .ToListAsync();
    }
}