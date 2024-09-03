using Microsoft.AspNetCore.DataProtection;
using zamren_management_system.Areas.Security.Interfaces;

namespace zamren_management_system.Areas.Common.Services;

public class CypherService : ICypherService
{
    private readonly IDataProtector _protector;

    public CypherService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("CypherServiceProtection");
    }

    public string Encrypt(object? val)
    {
        if (val == null) return string.Empty;
        var s = val.ToString();
        if (string.IsNullOrEmpty(s)) s = string.Empty;
        return _protector.Protect(s);
    }

    public string Decrypt(object? val)
    {
        if (val == null) return string.Empty;
        var s = val.ToString();
        if (string.IsNullOrEmpty(s)) s = string.Empty;
        if (Guid.TryParse(s, out _))
            return s;
        return _protector.Unprotect(s);
    }
}