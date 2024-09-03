using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AuthContext _context;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(AuthContext context, ILogger<OrganizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(Organization organization)
    {
        try
        {
            await _context.Organizations.AddAsync(organization);
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

    public async Task<IdentityResult> UpdateAsync(Organization organization)
    {
        try
        {
            _context.Organizations.Update(organization);
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

    public async Task<Organization?> FindByIdAsync(string id)
    {
        return await _context.Organizations.FindAsync(id);
    }
    
    public async Task<List<Organization>> FindAllExceptAsync(string id)
    {
        return await _context.Organizations.Where(o => o.Id != id).ToListAsync();
    }

    public async Task<List<Organization>> FindAllAsync()
    {
        return await _context.Organizations.ToListAsync();
    }

    public async Task<ICollection<Branch>> FindBranchesAsync(string organizationId)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .Where(b => b.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<ICollection<Department>> FindDepartmentsAsync(string organizationId)
    {
        var branches = await FindBranchesAsync(organizationId);
        var departments = new List<Department>();
        foreach (var branch in branches)
        {
            var branchDepartments = await _context.Departments
                .Where(d => d.BranchId == branch.Id)
                .ToListAsync();
            departments.AddRange(branchDepartments);
        }

        return departments;
    }

    public async Task<ICollection<Office>> FindOfficesAsync(string organizationId)
    {
        var departments = await FindDepartmentsAsync(organizationId);
        var offices = new List<Office>();
        foreach (var department in departments)
        {
            var departmentOffices = await _context.Offices
                .Where(o => o.DepartmentId == department.Id)
                .ToListAsync();
            offices.AddRange(departmentOffices);
        }

        return offices;
    }

    public async Task<ICollection<ApplicationUser>> FindUsersAsync(string organizationId)
    {
        var offices = await FindOfficesAsync(organizationId);
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

    public async Task<bool> OrganizationNameExistsAsync(string name)
    {
        return await _context.Organizations.AnyAsync(e => e.Name == name);
    }

    public async Task<bool> OrganizationNameExistsExceptAsync(string name, string id)
    {
        return await _context.Organizations.AnyAsync(e => e.Name == name && e.Id != id);
    }

    public async Task<bool> OrganizationIdExistsAsync(string id)
    {
        return await _context.Organizations.AnyAsync(e => e.Id == id);
    }

    public async Task<IdentityResult> DeleteAsync(Organization organization)
    {
        try
        {
            _context.Organizations.Remove(organization);
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
}