using Pusula.Student.Automation.Localization;
using Volo.Abp.AspNetCore.Mvc;
namespace Pusula.Student.Automation.Controllers;
public abstract class AutomationController : AbpControllerBase
{
    protected AutomationController()
    {
        LocalizationResource = typeof(AutomationResource);
    }
}
