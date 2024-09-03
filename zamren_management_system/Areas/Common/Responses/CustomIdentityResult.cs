using Microsoft.AspNetCore.Identity;

namespace zamren_management_system.Areas.Common.Responses;

public class CustomIdentityResult
{
    public bool Succeeded { get; set; }
    public IEnumerable<IdentityError> Errors { get; set; } = new List<IdentityError>();
    public string SuccessDescription { get; set; } = string.Empty;

    public static CustomIdentityResult Success(string description = "")
    {
        return new CustomIdentityResult { Succeeded = true, SuccessDescription = description };
    }

    public static CustomIdentityResult Failed(params IdentityError[] errors)
    {
        return new CustomIdentityResult { Succeeded = false, Errors = errors };
    }

    public override string ToString()
    {
        if (Succeeded)
        {
            return string.IsNullOrEmpty(SuccessDescription) ? "Succeeded" : $"Succeeded: {SuccessDescription}";
        }
        else
        {
            return $"Failed: {string.Join(", ", Errors.Select(e => e.Description))}";
        }
    }
}