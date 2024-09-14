using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Workflow.Dto;

public class WkfProcessStepDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PrivilegeDto? Privilege { get; set; }
    public WkfProcessDto? Process { get; set; }
    public bool? IsInitialStep { get; set; }
    public string? IsInitialStepString { get; set; }
    public bool? IsFinalStep { get; set; }
    public string? IsFinalStepString { get; set; }
    public bool? IsAutoApproved { get; set; }
    public bool? IsDepartmentHeadApproved { get; set; }
    public string? IsAutoApprovedString { get; set; }
    public string? IsDepartmentHeadApprovedString { get; set; }
    public string? Ordered { get; set; }
    public int? Order { get; set; }
    public bool HasConfigurationError { get; set; }
    public WkfProcessStepDto? PreviousStep { get; set; }
    public WkfProcessStepDto? NextStep { get; set; }
    // public WkfProcessDto? NextProcess { get; set; }
    // public WkfProcessDto? PrevProcess { get; set; }
    public string? RequestMap { get; set; }
    public int? SlaHours { get; set; }
    public RoleDto? Role { get; set; }
    public UserDto? ActioningUser { get; set; }
    public UserDto? SelectedUser { get; set; }
    public OfficeDto? Office { get; set; }
    public DepartmentDto? Department { get; set; }
    public BranchDto? Branch { get; set; }
    public OrganizationDto? Organization { get; set; }
    public string? PreviousStepId { get; set; }
    public string? NextStepId { get; set; }
    // public string? NextProcessId { get; set; }
    // public string? PrevProcessId { get; set; }
    public List<OfficeDto>? Offices { get; set; }
    public List<DepartmentDto>? Departments { get; set; }
    public List<BranchDto>? Branches { get; set; }
    public List<OrganizationDto>? Organizations { get; set; }
    public List<RoleDto>? Roles { get; set; }
    public List<WkfProcessStepDto>? Steps { get; set; }
    public List<WkfProcessDto>? Processes { get; set; }
    public List<PrivilegeDto>? Privileges { get; set; }
    public List<SlaHoursDto>? SlaHoursList { get; set; }
}