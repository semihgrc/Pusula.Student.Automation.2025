using Xunit;

namespace Pusula.Student.Automation.EntityFrameworkCore;

[CollectionDefinition(AutomationTestConsts.CollectionDefinitionName)]
public class AutomationEntityFrameworkCoreCollection : ICollectionFixture<AutomationEntityFrameworkCoreFixture>
{

}
