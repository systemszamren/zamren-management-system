using System.ComponentModel;

namespace zamren_management_system.Areas.Common.Enums;

public enum IdentityType
{
    [Description("National Registration Card (NRC)")]
    NRC,
    [Description("Passport")] Passport,
    [Description("Driver's License")] DriversLicense
}