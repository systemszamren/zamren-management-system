using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Models;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface IEmailVerificationService
{
    public Task<IdentityResult> CreateAsync(EmailVerification emailVerification);
    public Task<IdentityResult> UpdateAsync(EmailVerification emailVerification);

    public Task<EmailVerification?> FindByTokenAsync(string token);

    public Task<EmailVerification?> FindByUserIdAsync(string userId);

    public Task<List<EmailVerification>> FindAllByUserIdAsync(string userId);
}