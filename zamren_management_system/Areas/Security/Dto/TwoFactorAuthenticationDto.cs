namespace zamren_management_system.Areas.Security.Dto;

public class TwoFactorAuthenticationDto
{
    public int Counter { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool Is2faEnabled { get; set; }
}