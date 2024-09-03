namespace zamren_management_system.Areas.Security.Dto;

public class ExternalLoginDto
{
    public int Counter { get; set; }
    public string? LoginProvider { get; set; }
    public string? ProviderDisplayName { get; set; }
}