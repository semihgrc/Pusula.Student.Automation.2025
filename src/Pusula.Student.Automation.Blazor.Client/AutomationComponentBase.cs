using Pusula.Student.Automation.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Pusula.Student.Automation.Blazor.Client;

public abstract class AutomationComponentBase : AbpComponentBase
{
    protected AutomationComponentBase()
    {
        LocalizationResource = typeof(AutomationResource);
    }
}
