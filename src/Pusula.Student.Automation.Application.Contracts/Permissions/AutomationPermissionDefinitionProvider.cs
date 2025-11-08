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
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AutomationResource>(name);
    }
}
