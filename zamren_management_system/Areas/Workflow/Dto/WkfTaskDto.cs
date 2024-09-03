using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Workflow.Dto;

public class WkfTaskDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public int Counter { get; set; }
    public string? Reference { get; set; }
    public WkfTaskDto? ParentTask { get; set; }
    public WkfProcessDto? CurrentProcess { get; set; }
    public DateTimeOffset CurrentProcessStartDate { get; set; }
    public DateTimeOffset? CurrentProcessEndDate { get; set; }
    public WkfProcessStepDto? CurrentStep { get; set; }
    public DateTimeOffset CurrentStepStartedDate { get; set; }
    public DateTimeOffset? CurrentStepExpectedEndDate { get; set; }
    public UserDto? TaskStartedByUser { get; set; }
    public UserDto? CurrentActioningUser { get; set; }
    public UserDto? PreviousActioningUser { get; set; }
    public bool? IsOpen { get; set; }
    public bool? WasSentBack { get; set; }
    public WkfProcessStepDto? SentBackAtStep { get; set; }
    public List<WkfCommentDto>? Comments { get; set; }
    public List<WkfAttachmentDto>? Attachments { get; set; }
}