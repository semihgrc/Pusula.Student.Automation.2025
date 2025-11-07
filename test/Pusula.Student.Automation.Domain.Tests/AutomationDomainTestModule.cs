using Volo.Abp.Modularity;

namespace Pusula.Student.Automation;

[DependsOn(
    typeof(AutomationDomainModule),
    typeof(AutomationTestBaseModule)
)]
public class AutomationDomainTestModule : AbpModule
{

}
