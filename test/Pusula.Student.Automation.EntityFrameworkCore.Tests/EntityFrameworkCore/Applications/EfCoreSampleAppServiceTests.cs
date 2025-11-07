using Pusula.Student.Automation.Samples;
using Xunit;

namespace Pusula.Student.Automation.EntityFrameworkCore.Applications;

[Collection(AutomationTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AutomationEntityFrameworkCoreTestModule>
{

}
