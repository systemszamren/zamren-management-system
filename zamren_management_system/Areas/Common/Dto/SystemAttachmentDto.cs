namespace zamren_management_system.Areas.Common.Dto;

public class SystemAttachmentDto
{
    public string? Id { get; set; }
    public string FilePath { get; set; }
    public string SystemFileName { get; set; }
    public string CustomFileName { get; set; }
    public string OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string FileExtension { get; set; }
}