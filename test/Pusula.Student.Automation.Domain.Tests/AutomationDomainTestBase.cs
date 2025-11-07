using Volo.Abp.Modularity;

namespace Pusula.Student.Automation;

/* Inherit from this class for your domain layer tests. */
public abstract class AutomationDomainTestBase<TStartupModule> : AutomationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
