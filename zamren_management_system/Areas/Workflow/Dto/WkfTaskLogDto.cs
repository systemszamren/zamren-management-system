using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Workflow.Dto;

public class WkfTaskLogDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public int Counter { get; set; }
    public WkfTaskDto? Task { get; set; }
    public WkfProcessStepDto? Step { get; set; }
    public UserDto? ActioningUser { get; set; }
    public UserDto? NextActioningUser { get; set; }
    public string? Action { get; set; }
    public DateTimeOffset? ActionDate { get; set; }
}