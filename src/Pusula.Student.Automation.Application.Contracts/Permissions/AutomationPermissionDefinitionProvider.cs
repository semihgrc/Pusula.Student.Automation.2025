using Pusula.Student.Automation.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Pusula.Student.Automation.Permissions;

public class AutomationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AutomationPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(AutomationPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AutomationResource>(name);
    }
}
