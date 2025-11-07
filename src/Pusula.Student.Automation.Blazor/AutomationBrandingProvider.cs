using Microsoft.Extensions.Localization;
using Pusula.Student.Automation.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Pusula.Student.Automation.Blazor;

[Dependency(ReplaceServices = true)]
public class AutomationBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AutomationResource> _localizer;

    public AutomationBrandingProvider(IStringLocalizer<AutomationResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
