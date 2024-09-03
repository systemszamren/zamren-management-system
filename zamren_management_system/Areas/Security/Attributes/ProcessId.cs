namespace zamren_management_system.Areas.Security.Attributes;

public class ProcessId : Attribute
{
    public string? Id { get; private set; }

    public ProcessId(string? id)
    {
        Id = id;
    }
}