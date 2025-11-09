using Pusula.Student.Automation.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Pusula.Student.Automation.Permissions;

public class AutomationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AutomationPermissions.GroupName);
        myGroup.AddPermission(AutomationPermissions.AdminManagement, L("Permission:AdminManagement"));
        myGroup.AddPermission(AutomationPermissions.TeacherPortal, L("Permission:TeacherPortal"));
        myGroup.AddPermission(AutomationPermissions.StudentPortal, L("Permission:StudentPortal"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AutomationResource>(name);
    }
}
