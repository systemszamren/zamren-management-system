using System.ComponentModel;

namespace zamren_management_system.Areas.Workflow.Enums;

public enum WkfAction
{
    [Description("Initiate Task")] Initiate,
    
    [Description("Approve Task")] Approve,

    // [Description("Reject Task")] Reject,

    [Description("Send Back Task")] SendBack,

    [Description("Close Task")] Close,

    [Description("Reopen Task")] Reopen,

    [Description("Reassign Task")] Reassign,
}