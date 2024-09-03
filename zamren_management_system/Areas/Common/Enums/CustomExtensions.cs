using System.ComponentModel;
using zamren_management_system.Areas.Common.Attributes;
using zamren_management_system.Areas.Security.Attributes;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Enums.PurchaseRequisitionProcess;

namespace zamren_management_system.Areas.Common.Enums;

public static class CustomExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        return Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute
            ? value.ToString()
            : attribute.Description;
    }

    public static string? GetModuleCode(this ModuleConstant module)
    {
        // Get the type of the enum
        var type = module.GetType();

        // Get the field info for this type
        var fieldInfo = type.GetField(module.ToString());

        // Get the ModuleCode attribute if it exists
        if (fieldInfo == null) return string.Empty;
        var attribute =
            fieldInfo.GetCustomAttributes(typeof(ModuleCode), false).FirstOrDefault() as ModuleCode;

        // Return the code value if the attribute exists; otherwise, null
        return attribute?.Code;
    }
    
    //GetProcessId
    public static string? GetProcessId(this ProcessConstant process)
    {
        // Get the type of the enum
        var type = process.GetType();

        // Get the field info for this type
        var fieldInfo = type.GetField(process.ToString());

        // Get the ProcessId attribute if it exists
        if (fieldInfo == null) return string.Empty;
        var attribute =
            fieldInfo.GetCustomAttributes(typeof(ProcessId), false).FirstOrDefault() as ProcessId;

        // Return the id value if the attribute exists; otherwise, null
        return attribute?.Id;
    }
    
    //GetProcessStepId
    public static string? GetProcessStepId(this ProcessStepConstant step)
    {
        // Get the type of the enum
        var type = step.GetType();

        // Get the field info for this type
        var fieldInfo = type.GetField(step.ToString());

        // Get the StepId attribute if it exists
        if (fieldInfo == null) return string.Empty;
        var attribute =
            fieldInfo.GetCustomAttributes(typeof(StepId), false).FirstOrDefault() as StepId;

        // Return the id value if the attribute exists; otherwise, null
        return attribute?.Id;
    }

    public static string? GetDirectoryName(this AttachmentDirectory directory)
    {
        // Get the type of the enum
        var type = directory.GetType();

        // Get the field info for this type
        var fieldInfo = type.GetField(directory.ToString());

        // Get the AttachmentDirectory attribute if it exists
        if (fieldInfo == null) return string.Empty;
        var attribute =
            fieldInfo.GetCustomAttributes(typeof(DirectoryName), false).FirstOrDefault() as DirectoryName;

        // Return the name value if the attribute exists; otherwise, null
        return attribute?.Value;
    }
}