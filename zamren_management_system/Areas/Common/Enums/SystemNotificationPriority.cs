using System.ComponentModel;

namespace zamren_management_system.Areas.Common.Enums;

public enum SystemNotificationPriority
{
    [Description("Low")] Low = 1,

    [Description("Medium")] Medium = 2,

    [Description("High")] High = 3,
}