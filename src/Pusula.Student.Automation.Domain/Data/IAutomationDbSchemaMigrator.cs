using System.Threading.Tasks;

namespace Pusula.Student.Automation.Data;

public interface IAutomationDbSchemaMigrator
{
    Task MigrateAsync();
}
