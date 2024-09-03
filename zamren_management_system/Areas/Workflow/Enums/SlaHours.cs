using System.ComponentModel;

namespace zamren_management_system.Areas.Workflow.Enums;

public enum SlaHours
{
    [Description("1 Hour")] _1Hour = 1,

    [Description("12 Hours (Half Working Day)")]
    _12Hours = 12,

    [Description("24 Hours (1 Working Day)")]
    _24Hours = 24,

    [Description("48 Hours (2 Working Days)")]
    _48Hours = 48,

    [Description("72 Hours (3 Working Days)")]
    _72Hours = 72,

    [Description("120 Hours (5 Working Days)")]
    _120Hours = 120,
}