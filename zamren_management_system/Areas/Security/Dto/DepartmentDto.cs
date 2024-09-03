namespace zamren_management_system.Areas.Security.Dto;

public class DepartmentDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public string? Name { get; set; }
    public int Counter { get; set; }
    public string? Description { get; set; }
    
    public BranchDto Branch { get; set; }
    public string BranchId { get; set; }
}