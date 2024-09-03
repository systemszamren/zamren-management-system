using zamren_management_system.Areas.Common.Dto;

namespace zamren_management_system.Areas.Workflow.Dto;

public class WkfAttachmentDto
{
    public string? Id { get; set; }
    public WkfTaskDto? Task { get; set; }
    public WkfProcessStepDto? Step { get; set; }
    public SystemAttachmentDto SystemAttachment { get; set; }
}