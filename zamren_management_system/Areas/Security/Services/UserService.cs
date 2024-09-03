using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Security.Services;

public class UserService : IUserService
{
    private readonly AuthContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AuthContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllExceptByIdAsync(string userId)
    {
        try
        {
            return await _context.Users.Where(u => u.Id != userId).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllExceptByEmailAsync(string email)
    {
        try
        {
            return await _context.Users.Where(u => u.Email != email).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllBySupervisorIdAsync(string supervisorUserId)
    {
        try
        {
            return await _context.Users.Where(u => u.SupervisorUserId == supervisorUserId).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllByPhoneNumberExceptByUserIdAsync(string phoneNumber,
        string userId)
    {
        try
        {
            return await _context.Users.Where(u => u.PhoneNumber == phoneNumber && u.Id != userId).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllByEmailAddressExceptByUserIdAsync(string email,
        string userId)
    {
        try
        {
            return await _context.Users.Where(u => u.Email == email && u.Id != userId).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllEmployeesAsync()
    {
        try
        {
            return await _context.Users.Where(u => u.IsEmployee).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }
    
    public async Task<IEnumerable<ApplicationUser>> FindAllEmployeesExceptAsync(string userId)
    {
        try
        {
            return await _context.Users.Where(u => u.IsEmployee && u.Id != userId).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }
    
    public async Task<IEnumerable<ApplicationUser>> FindAllEmployeesAndCanActionWkfTasksAsync()
    {
        try
        {
            return await _context.Users
                .Where(u => u.IsEmployee && u.CanActionWkfTasks)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<ApplicationUser>();
        }
    }
}