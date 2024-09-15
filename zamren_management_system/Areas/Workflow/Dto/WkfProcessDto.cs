using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Workflow.Dto;

public class WkfProcessDto
{
    
     public string? Id { get; set; }
    public string? PlainId { get; set; }
     public int Counter { get; set; }
     public string? Name { get; set; }
     public string? Description { get; set; }
     public string? ParentProcessId { get; set; }
     public ModuleDto? Module { get; set; }
     public string? IsChildProcessString { get; set; }
}