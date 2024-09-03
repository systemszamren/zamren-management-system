using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Services;

public class PasswordHistoryService : IPasswordHistoryService
{
    private readonly AuthContext _context;
    private readonly ILogger<PasswordHistoryService> _logger;

    public PasswordHistoryService(AuthContext context, ILogger<PasswordHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(PasswordHistory passwordHistory)
    {
        try
        {
            await _context.PasswordHistories.AddAsync(passwordHistory);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "AddPasswordHistoryAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }

    public async Task<PasswordHistory?> FindByIdAsync(string id)
    {
        return await _context.PasswordHistories.FindAsync(id);
    }

    public async Task<List<PasswordHistoryDto>> GetPasswordHistoryDtos(string userId)
    {
        var passwordHistories = await _context.PasswordHistories
            .Include(ph => ph.User)
            .Where(ph => ph.UserId == userId)
            .ToListAsync();

        return passwordHistories.Select(ph => new PasswordHistoryDto
            {
                Id = ph.Id,
                UserId = ph.UserId,
                PasswordExpiryDate = ph.PasswordExpiryDate,
                PasswordCreatedDate = ph.PasswordCreatedDate,
                Status = ph.Status
            })
            .ToList();
    }

    public async Task<List<PasswordHistory>> GetPasswordHistories(string userId)
    {
        return await _context.PasswordHistories
            .Include(ph => ph.User)
            .Where(ph => ph.UserId == userId)
            .ToListAsync();
    }

    public async Task<IdentityResult> UpdateAsync(PasswordHistory passwordHistory)
    {
        try
        {
            _context.PasswordHistories.Update(passwordHistory);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdatePasswordHistoryAsync",
                Description = "An error occurred while performing the operation"
            });
        }
    }
}