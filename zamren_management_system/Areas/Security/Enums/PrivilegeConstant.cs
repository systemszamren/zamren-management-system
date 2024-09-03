using System.ComponentModel;

namespace zamren_management_system.Areas.Security.Enums;

public enum PrivilegeConstant
{
    //add description to each enum
    [Description("Access Admin Dashboard")]
    DASH_ACCESS_ADMIN_DASHBOARD,

    [Description("Access Client Dashboard")]
    DASH_ACCESS_CLIENT_DASHBOARD,

    [Description("Manage System Users")] SEC_MANAGE_SYSTEM_USERS,

    [Description("Manage System Roles")] SEC_MANAGE_SYSTEM_ROLES,

    [Description("Manage System Privileges")]
    SEC_MANAGE_SYSTEM_PRIVILEGES,

    [Description("Manage System Modules")] SEC_MANAGE_SYSTEM_MODULES,

    [Description("Manage System Organizations")]
    SEC_MANAGE_SYSTEM_ORGANIZATIONS,

    [Description("Manage System Branches")]
    SEC_MANAGE_SYSTEM_BRANCHES,

    [Description("Manage System Departments")]
    SEC_MANAGE_SYSTEM_DEPARTMENTS,

    [Description("Manage System Offices")] SEC_MANAGE_SYSTEM_OFFICES,

    [Description("Manage Client Account")] CLIENT_MANAGE_ACCOUNT,

    [Description("Manage System Workflow")]
    WKF_MANAGE_SYSTEM_WORKFLOW,
    PROC_INITIATE_PURCHASE_REQUISITION,
    SEC_EMPLOYEE,
}