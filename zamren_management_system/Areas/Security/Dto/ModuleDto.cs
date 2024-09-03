using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.Dto;

public class ModuleDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int Counter { get; set; }
    public string? Description { get; set; }
    public List<Privilege> Privileges { get; set; }
}