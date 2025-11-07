using Volo.Abp.Modularity;

namespace Pusula.Student.Automation;

[DependsOn(
    typeof(AutomationApplicationModule),
    typeof(AutomationDomainTestModule)
)]
public class AutomationApplicationTestModule : AbpModule
{

}
