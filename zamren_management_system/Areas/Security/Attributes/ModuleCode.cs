namespace zamren_management_system.Areas.Security.Attributes;

public class ModuleCode : Attribute
{
    public string? Code { get; private set; }

    public ModuleCode(string? code)
    {
        Code = code;
    }
}