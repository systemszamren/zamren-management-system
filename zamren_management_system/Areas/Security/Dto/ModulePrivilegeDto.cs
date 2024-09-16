using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Dto;

public class ModulePrivilegeDto
{
    public int Counter { get; set; }
    public ModuleDto Module { get; set; }
    public PrivilegeDto Privilege { get; set; }
}