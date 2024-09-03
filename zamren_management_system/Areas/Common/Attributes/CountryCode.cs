namespace zamren_management_system.Areas.Common.Attributes;

public class CountryCode : Attribute
{
    public string? Value { get; private set; }

    public CountryCode(string? value)
    {
        Value = value;
    }
}