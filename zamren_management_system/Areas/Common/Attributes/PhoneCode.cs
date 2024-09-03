namespace zamren_management_system.Areas.Common.Attributes;

public class PhoneCode : Attribute
{
    public string? Value { get; private set; }

    public PhoneCode(string? value)
    {
        Value = value;
    }
}