namespace zamren_management_system.Areas.Security.Attributes;

public class StepId : Attribute
{
    public string? Id { get; private set; }

    public StepId(string? id)
    {
        Id = id;
    }
}