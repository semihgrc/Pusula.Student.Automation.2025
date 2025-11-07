using Pusula.Student.Automation.Samples;
using Xunit;

namespace Pusula.Student.Automation.EntityFrameworkCore.Domains;

[Collection(AutomationTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AutomationEntityFrameworkCoreTestModule>
{

}
