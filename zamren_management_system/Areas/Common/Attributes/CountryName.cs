namespace zamren_management_system.Areas.Common.Attributes;

public class CountryName : Attribute
{
    public string? Value { get; private set; }

    public CountryName(string? value)
    {
        Value = value;
    }
}