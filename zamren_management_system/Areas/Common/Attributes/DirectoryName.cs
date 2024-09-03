namespace zamren_management_system.Areas.Common.Attributes;

public class DirectoryName : Attribute
{
    public string? Value { get; private set; }

    public DirectoryName(string? value)
    {
        Value = value;
    }
}