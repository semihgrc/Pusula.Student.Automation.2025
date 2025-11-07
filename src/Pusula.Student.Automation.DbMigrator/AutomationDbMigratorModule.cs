using Pusula.Student.Automation.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Pusula.Student.Automation.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AutomationEntityFrameworkCoreModule),
    typeof(AutomationApplicationContractsModule)
    )]
public class AutomationDbMigratorModule : AbpModule
{
}
