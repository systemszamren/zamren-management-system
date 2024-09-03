using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Common.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly AuthContext _context;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(AuthContext context, ILogger<EmailVerificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(EmailVerification emailVerification)
    {
        try
        {
            await _context.EmailVerifications.AddAsync(emailVerification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateAsync",
                Description = "Failed to save email verification"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(EmailVerification emailVerification)
    {
        try
        {
            _context.EmailVerifications.Update(emailVerification);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateAsync",
                Description = "Failed to update email verification"
            });
        }
    }

    public async Task<EmailVerification?> FindByTokenAsync(string token)
    {
        return await _context.EmailVerifications.FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<EmailVerification?> FindByUserIdAsync(string userId)
    {
        return await _context.EmailVerifications.FirstOrDefaultAsync(x => x.UserId == userId);
    }
    
    public async Task<List<EmailVerification>> FindAllByUserIdAsync(string userId)
    {
        return await _context.EmailVerifications.Where(x => x.UserId == userId).ToListAsync();
    }
}