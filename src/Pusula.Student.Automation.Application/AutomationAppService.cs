using System;
using System.Collections.Generic;
using System.Text;
using Pusula.Student.Automation.Localization;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation;

/* Inherit your application services from this class.
 */
public abstract class AutomationAppService : ApplicationService
{
    protected AutomationAppService()
    {
        LocalizationResource = typeof(AutomationResource);
    }
}
