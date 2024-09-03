using zamren_management_system.Areas.Security.Dto;

namespace zamren_management_system.Areas.Workflow.Dto;

public class WkfCommentDto
{
    public string? Id { get; set; }
    public string? Comment { get; set; }

    public WkfTaskDto? Task { get; set; }

    public WkfProcessStepDto? Step { get; set; }

    public UserDto? CommentedBy { get; set; }
    
    public DateTimeOffset? CommentedDate { get; set; }
    public string? TimeAgo { get; set; }
    
    public bool? MostRecent { get; set; }
    
    public List<WkfAttachmentDto>? WkfAttachments { get; set; }
}