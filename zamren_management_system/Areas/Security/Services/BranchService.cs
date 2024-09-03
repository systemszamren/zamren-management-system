using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class BranchService : IBranchService
{
    private readonly AuthContext _context;
    private readonly ILogger<BranchService> _logger;

    public BranchService(AuthContext context, ILogger<BranchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(Branch branch)
    {
        try
        {
            await _context.Branches.AddAsync(branch);
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

    public async Task<IdentityResult> UpdateAsync(Branch branch)
    {
        try
        {
            _context.Branches.Update(branch);
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

    public async Task<IdentityResult> DeleteAsync(Branch branch)
    {
        try
        {
            _context.Branches.Remove(branch);
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

    public async Task<Branch?> FindByIdAsync(string branchId)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == branchId);
    }

    public async Task<List<Branch>> FindAllAsync()
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .ToListAsync();
    }

    //FindAllExceptAsync
    public async Task<List<Branch>> FindAllExceptAsync(string branchId)
    {
        return await _context.Branches.Where(b => b.Id != branchId).ToListAsync();
    }

    public async Task<ICollection<Department>> FindDepartmentsAsync(string branchId)
    {
        return await _context.Departments.Where(d => d.BranchId == branchId).ToListAsync();
    }

    public async Task<ICollection<Office>> FindOfficesAsync(string branchId)
    {
        var departments = await FindDepartmentsAsync(branchId);
        var offices = new List<Office>();

        foreach (var department in departments)
        {
            var departmentOffices =
                await _context.Offices.Where(o => o.DepartmentId == department.Id).ToListAsync();
            offices.AddRange(departmentOffices);
        }

        return offices;
    }

    public async Task<ICollection<ApplicationUser>> FindUsersAsync(string branchId)
    {
        var offices = await FindOfficesAsync(branchId);
        var users = new List<ApplicationUser>();

        foreach (var office in offices)
        {
            var officeUsers = await _context.UserOffices
                .Where(uo => uo.OfficeId == office.Id)
                .Select(uo => uo.User)
                .ToListAsync();
            users.AddRange(officeUsers);
        }

        return users;
    }

    public async Task<bool> BranchNameExistsAsync(string name)
    {
        return await _context.Branches.AnyAsync(b => b.Name == name);
    }

    //Checks if record exists except for the record with the given ID
    public async Task<bool> BranchNameExistsExceptAsync(string name, string id)
    {
        return await _context.Branches.AnyAsync(b => b.Name == name && b.Id != id);
    }

    public async Task<bool> BranchIdExistsAsync(string branchId)
    {
        return await _context.Branches.AnyAsync(b => b.Id == branchId);
    }

    //Checks if record exists except for the record with the given ID
    public async Task<bool> BranchIdExistsExceptAsync(string branchId, string id)
    {
        return await _context.Branches.AnyAsync(b => b.Id == branchId && b.Id != id);
    }
    
    //FindByOrganizationIdAsync
    public async Task<ICollection<Branch>> FindByOrganizationIdAsync(string organizationId)
    {
        return await _context.Branches.Where(b => b.OrganizationId == organizationId).ToListAsync();
    }
}