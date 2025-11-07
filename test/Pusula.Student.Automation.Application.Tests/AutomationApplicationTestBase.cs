using Volo.Abp.Modularity;

namespace Pusula.Student.Automation;

public abstract class AutomationApplicationTestBase<TStartupModule> : AutomationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
