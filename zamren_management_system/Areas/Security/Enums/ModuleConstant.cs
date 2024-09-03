using System.ComponentModel;
using zamren_management_system.Areas.Security.Attributes;

namespace zamren_management_system.Areas.Security.Enums;

public enum ModuleConstant
{
    [Description("General Module")] [ModuleCode("GEN")]
    GENERAL,

    [Description("Security Module")] [ModuleCode("SEC")]
    SECURITY,

    [Description("Workflow Module")] [ModuleCode("WKF")]
    WORKFLOW,

    [Description("Procurment Module")] [ModuleCode("PROC")] 
    PROCURMENT,
}