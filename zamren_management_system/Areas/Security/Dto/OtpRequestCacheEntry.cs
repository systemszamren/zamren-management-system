namespace zamren_management_system.Areas.Security.Dto;

public class OtpRequestCacheEntry
{
    public int RequestCount { get; set; }
    public DateTimeOffset LastRequestTime { get; set; }
}